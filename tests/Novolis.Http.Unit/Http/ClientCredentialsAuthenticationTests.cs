using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Novolis.Http.Authentication.OAuth;
using Novolis.Http.Authentication.Tokens;
using Novolis.Http.Client;
using Novolis.Http.Unit.Infrastructure;

namespace Novolis.Http.Unit;

public sealed class ClientCredentialsAuthenticationTests
{
    [Test]
    public async Task SendAsync_posts_client_credentials_and_reuses_the_cached_token()
    {
        var tokenCalls = 0;
        string? tokenForm = null;
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = async (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    Interlocked.Increment(ref tokenCalls);
                    tokenForm = await request.Content!.ReadAsStringAsync();
                    return JsonOk("""{"access_token":"server-token","token_type":"Bearer","expires_in":3600}""");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>(client =>
                client.BaseAddress = new Uri("https://warehouse.example/"))
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
                options.Scope = "warehouse.read";
            });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient<WarehouseApi>();
        using var first = await client.GetAsync("https://warehouse.example/bins");
        using var second = await client.GetAsync("https://warehouse.example/bins");

        await Assert.That(tokenCalls).IsEqualTo(1);
        await Assert.That(tokenForm).IsNotNull();
        await Assert.That(tokenForm!).Contains("grant_type=client_credentials");
        await Assert.That(tokenForm).Contains("client_id=warehouse");
        var tokenPost = handler.SentRequests.First(request => request.RequestUri!.AbsolutePath == "/oauth/token");
        await Assert.That(tokenPost.Headers.Authorization).IsNull();
        await Assert.That(handler.SentRequests.Count(request => request.RequestUri!.AbsolutePath == "/bins")).IsEqualTo(2);
        await Assert.That(handler.SentRequests.Last(request => request.RequestUri!.AbsolutePath == "/bins")
            .Headers.Authorization!.Parameter).IsEqualTo("server-token");
    }

    [Test]
    public async Task SendAsync_reacquires_after_expiry_without_refresh_token()
    {
        var tokenCalls = 0;
        var tokenForms = new List<string>();
        var time = new FakeTimeProvider();
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = async (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    Interlocked.Increment(ref tokenCalls);
                    tokenForms.Add(await request.Content!.ReadAsStringAsync());
                    return JsonOk("""{"access_token":"token","expires_in":120}""");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
        };
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(time);
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>()
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient<WarehouseApi>();
        using var first = await client.GetAsync("https://warehouse.example/bins");
        time.Advance(TimeSpan.FromSeconds(61));
        using var second = await client.GetAsync("https://warehouse.example/bins");

        await Assert.That(tokenCalls).IsEqualTo(2);
        await Assert.That(tokenForms.All(form => form.Contains("grant_type=client_credentials"))).IsTrue();
    }

    [Test]
    public async Task Concurrent_calls_share_one_token_request()
    {
        var tokenCalls = 0;
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = async (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    Interlocked.Increment(ref tokenCalls);
                    await release.Task;
                    return JsonOk("""{"access_token":"once","expires_in":3600}""");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>()
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var first = factory.CreateClient<WarehouseApi>().GetAsync("https://warehouse.example/bins");
        var second = factory.CreateClient<WarehouseApi>().GetAsync("https://warehouse.example/bins");
        while (Volatile.Read(ref tokenCalls) == 0)
        {
            await Task.Delay(10);
        }

        await Task.Delay(20);
        release.SetResult();
        await Task.WhenAll(first, second);
        first.Result.Dispose();
        second.Result.Dispose();

        await Assert.That(tokenCalls).IsEqualTo(1);
    }

    [Test]
    public async Task Unauthorized_retries_once_with_a_new_token()
    {
        var resourceCalls = 0;
        var tokenCalls = 0;
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    var token = Interlocked.Increment(ref tokenCalls) == 1 ? "first" : "second";
                    return Task.FromResult(JsonOk($$"""{"access_token":"{{token}}","expires_in":3600}"""));
                }

                var count = Interlocked.Increment(ref resourceCalls);
                return Task.FromResult(new HttpResponseMessage(
                    count == 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK));
            },
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>()
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        using var response = await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<WarehouseApi>()
            .GetAsync("https://warehouse.example/bins");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(resourceCalls).IsEqualTo(2);
        var resource = handler.SentRequests.Where(request => request.RequestUri!.AbsolutePath == "/bins").ToArray();
        await Assert.That(resource[0].Headers.Authorization!.Parameter).IsEqualTo("first");
        await Assert.That(resource[1].Headers.Authorization!.Parameter).IsEqualTo("second");
    }

    [Test]
    public async Task First_caller_cancel_does_not_fail_the_second_token_waiter()
    {
        var tokenCalls = 0;
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = async (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    Interlocked.Increment(ref tokenCalls);
                    await release.Task;
                    return JsonOk("""{"access_token":"shared","expires_in":3600}""");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>()
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        using var firstCts = new CancellationTokenSource();
        var first = factory.CreateClient<WarehouseApi>()
            .GetAsync("https://warehouse.example/bins", firstCts.Token);
        var second = factory.CreateClient<WarehouseApi>()
            .GetAsync("https://warehouse.example/bins");
        while (Volatile.Read(ref tokenCalls) == 0)
        {
            await Task.Delay(10);
        }

        firstCts.Cancel();
        release.SetResult();
        await Assert.ThrowsAsync<OperationCanceledException>(() => first);
        using var response = await second;

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(tokenCalls).IsEqualTo(1);
        await Assert.That(handler.SentRequests.Last(request => request.RequestUri!.AbsolutePath == "/bins")
            .Headers.Authorization!.Parameter).IsEqualTo("shared");
    }

    [Test]
    public async Task TokenEndpoint_rejects_plain_http_off_loopback()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>()
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "http://evil.example/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient<WarehouseApi>()
                .GetAsync("https://warehouse.example/bins"));

        await Assert.That(exception!.Message).Contains("https");
        await Assert.That(handler.SentRequests.Count).IsEqualTo(0);
    }

    [Test]
    public async Task TokenEndpoint_allows_loopback_http()
    {
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    return Task.FromResult(JsonOk("""{"access_token":"loopback","expires_in":3600}"""));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            },
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>()
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "http://127.0.0.1/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        using var response = await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<WarehouseApi>()
            .GetAsync("https://warehouse.example/bins");

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(handler.SentRequests.Any(request =>
            request.RequestUri!.Scheme == Uri.UriSchemeHttp
            && request.RequestUri.IsLoopback
            && request.RequestUri.AbsolutePath == "/oauth/token")).IsTrue();
    }

    [Test]
    public async Task Invalidate_clears_the_cached_token_for_the_key()
    {
        var tokenCalls = 0;
        var handler = new StubHttpMessageHandler
        {
            SendAsyncImpl = (request, _) =>
            {
                if (request.RequestUri!.AbsolutePath == "/oauth/token")
                {
                    Interlocked.Increment(ref tokenCalls);
                    return Task.FromResult(JsonOk("""{"access_token":"token","expires_in":3600}"""));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            },
        };
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<WarehouseApi>()
            .AddClientCredentialsAuthentication(options =>
            {
                options.TokenEndpoint = "https://login.example/oauth/token";
                options.ClientId = "warehouse";
                options.ClientSecret = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient<WarehouseApi>();
        using var first = await client.GetAsync("https://warehouse.example/bins");
        provider.GetRequiredService<IHttpTokenCache>().Invalidate<WarehouseApi>();
        using var second = await client.GetAsync("https://warehouse.example/bins");

        await Assert.That(tokenCalls).IsEqualTo(2);
    }

    private static HttpResponseMessage JsonOk(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    public sealed class WarehouseApi() : HttpClientKey;
}
