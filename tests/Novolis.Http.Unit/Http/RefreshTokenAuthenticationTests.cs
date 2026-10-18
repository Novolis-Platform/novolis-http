using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Novolis.Http.Authentication.OAuth;
using Novolis.Http.Client;
using Novolis.Http.Unit.Infrastructure;

namespace Novolis.Http.Unit;

public sealed class RefreshTokenAuthenticationTests
{
    [Test]
    public async Task SendAsync_uses_seed_access_token_then_refreshes()
    {
        string? tokenForm = null;
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = async (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    tokenForm = await request.Content!.ReadAsStringAsync();
                    return JsonOk(
                        """{"access_token":"rotated","refresh_token":"next-refresh","expires_in":3600}""");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
        };
        var time = new FakeTimeProvider();
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(time);
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<PartnerApi>()
            .AddRefreshTokenAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "partner-app";
                options.RefreshToken = "refresh-from-environment";
                options.AccessToken = "seed-token";
                options.ExpiresIn = 120;
            });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient<PartnerApi>();
        using var first = await client.GetAsync("https://partner.example/orders");
        await Assert.That(handler.SentRequests.Count(request => request.RequestUri!.AbsolutePath == "/oauth/token"))
            .IsEqualTo(0);
        await Assert.That(handler.SentRequests[0].Headers.Authorization!.Parameter).IsEqualTo("seed-token");

        time.Advance(TimeSpan.FromSeconds(61));
        using var second = await client.GetAsync("https://partner.example/orders");
        await Assert.That(tokenForm).IsNotNull();
        await Assert.That(tokenForm!).Contains("grant_type=refresh_token");
        await Assert.That(tokenForm).Contains("refresh_token=refresh-from-environment");
        await Assert.That(handler.SentRequests.Last(request => request.RequestUri!.AbsolutePath == "/orders")
            .Headers.Authorization!.Parameter).IsEqualTo("rotated");
    }

    [Test]
    public async Task SendAsync_uses_rotated_refresh_token_on_the_next_refresh()
    {
        var refreshBodies = new List<string>();
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = async (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    refreshBodies.Add(await request.Content!.ReadAsStringAsync());
                    var token = refreshBodies.Count == 1 ? "one" : "two";
                    var refresh = refreshBodies.Count == 1 ? "rotated-refresh" : "final-refresh";
                    return JsonOk($$"""{"access_token":"{{token}}","refresh_token":"{{refresh}}","expires_in":120}""");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
        };
        var time = new FakeTimeProvider();
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(time);
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<PartnerApi>()
            .AddRefreshTokenAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "partner-app";
                options.RefreshToken = "original-refresh";
            });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient<PartnerApi>();
        using var first = await client.GetAsync("https://partner.example/orders");
        time.Advance(TimeSpan.FromSeconds(61));
        using var second = await client.GetAsync("https://partner.example/orders");

        await Assert.That(refreshBodies[0]).Contains("refresh_token=original-refresh");
        await Assert.That(refreshBodies[1]).Contains("refresh_token=rotated-refresh");
    }

    [Test]
    public async Task Unauthorized_retries_with_the_rotated_refresh_token()
    {
        var refreshBodies = new List<string>();
        var resourceCalls = 0;
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = async (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    refreshBodies.Add(await request.Content!.ReadAsStringAsync());
                    var token = refreshBodies.Count == 1 ? "stale" : "fresh";
                    var refresh = refreshBodies.Count == 1 ? "rotated-refresh" : "final-refresh";
                    return JsonOk($$"""{"access_token":"{{token}}","refresh_token":"{{refresh}}","expires_in":3600}""");
                }

                var count = Interlocked.Increment(ref resourceCalls);
                return new HttpResponseMessage(count == 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK);
            },
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<PartnerApi>()
            .AddRefreshTokenAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "partner-app";
                options.RefreshToken = "seed-refresh";
            });

        await using var provider = services.BuildServiceProvider();
        using var response = await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<PartnerApi>()
            .GetAsync("https://partner.example/orders");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(refreshBodies.Count).IsEqualTo(2);
        await Assert.That(refreshBodies[0]).Contains("refresh_token=seed-refresh");
        await Assert.That(refreshBodies[1]).Contains("refresh_token=rotated-refresh");
        await Assert.That(refreshBodies[1].Contains("refresh_token=seed-refresh", StringComparison.Ordinal)).IsFalse();
        var resource = handler.SentRequests.Where(request => request.RequestUri!.AbsolutePath == "/orders").ToArray();
        await Assert.That(resource[0].Headers.Authorization!.Parameter).IsEqualTo("stale");
        await Assert.That(resource[1].Headers.Authorization!.Parameter).IsEqualTo("fresh");
    }

    [Test]
    public async Task Invalid_grant_clears_the_cache_and_fails()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":"invalid_grant"}""", Encoding.UTF8, "application/json"),
            }),
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<PartnerApi>()
            .AddRefreshTokenAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "partner-app";
                options.RefreshToken = "dead-refresh";
            });

        await using var provider = services.BuildServiceProvider();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient<PartnerApi>()
                .GetAsync("https://partner.example/orders"));

        await Assert.That(exception!.Message).Contains("invalid_grant");
    }

    private static HttpResponseMessage JsonOk(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    public sealed class PartnerApi() : HttpClientKey;
}
