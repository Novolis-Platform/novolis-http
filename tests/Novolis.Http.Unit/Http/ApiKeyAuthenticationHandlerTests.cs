using Microsoft.Extensions.DependencyInjection;
using Novolis.Http.Authentication.Api;
using Novolis.Http.Client;
using Novolis.Http.Unit.Infrastructure;

namespace Novolis.Http.Unit;

public sealed class ApiKeyAuthenticationHandlerTests
{
    [Test]
    public async Task SendAsync_adds_header_by_default()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<BillingApi>()
            .AddApiKeyAuthentication(options =>
            {
                options.ApiKey = "billing-key";
            });

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<BillingApi>()
            .GetAsync("https://billing.example/invoices");

        await Assert.That(handler.SentRequests[0].Headers.GetValues("X-Api-Key").Single())
            .IsEqualTo("billing-key");
    }

    [Test]
    public async Task SendAsync_prefixes_header_with_scheme()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<BillingApi>()
            .AddApiKeyAuthentication(options =>
            {
                options.HeaderName = "Authorization";
                options.Scheme = "ApiKey";
                options.ApiKey = "billing-key";
            });

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<BillingApi>()
            .GetAsync("https://billing.example/invoices");

        var authorization = handler.SentRequests[0].Headers.Authorization;
        await Assert.That(authorization).IsNotNull();
        await Assert.That(authorization!.Scheme).IsEqualTo("ApiKey");
        await Assert.That(authorization.Parameter).IsEqualTo("billing-key");
    }

    [Test]
    public async Task SendAsync_adds_query_parameter_instead_of_header()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<BillingApi>()
            .AddApiKeyAuthentication(options =>
            {
                options.QueryParameterName = "api_key";
                options.ApiKey = "billing-key";
            });

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<BillingApi>()
            .GetAsync("https://billing.example/invoices?page=1");

        var request = handler.SentRequests[0];
        await Assert.That(request.RequestUri!.Query).Contains("api_key=billing-key");
        await Assert.That(request.Headers.Contains("X-Api-Key")).IsFalse();
    }

    public sealed class BillingApi() : HttpClientKey;
}
