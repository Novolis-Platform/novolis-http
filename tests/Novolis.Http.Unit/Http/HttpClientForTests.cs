using Microsoft.Extensions.DependencyInjection;
using Novolis.Http.Client;
using Novolis.Http.Documents;
using Novolis.Http.Unit.Infrastructure;
using Novolis.Http.Variables;

namespace Novolis.Http.Unit;

public sealed class HttpClientForTests
{
    [Test]
    public async Task AddHttpClientFor_does_not_register_the_key_in_dependency_injection()
    {
        var services = new ServiceCollection();
        services.AddNovolisHttp();
        services.AddHttpClientFor<CatalogApi>(client =>
            client.BaseAddress = new Uri("https://catalog.example/"));

        await using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetService<CatalogApi>()).IsNull();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var typed = factory.CreateClient<CatalogApi>();
        var named = factory.CreateClient(new CatalogApi().GetClientName());
        await Assert.That(typed.BaseAddress!.ToString()).IsEqualTo("https://catalog.example/");
        await Assert.That(named.BaseAddress!.ToString()).IsEqualTo("https://catalog.example/");
    }

    [Test]
    public async Task SendAsync_typed_uses_the_key_client()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<CatalogApi>(client =>
            client.BaseAddress = new Uri("https://catalog.example/"));

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var document = new HttpRequestDocument { Method = "GET", Uri = "https://catalog.example/items" };

        using var response = await factory.SendAsync<CatalogApi>(
            document,
            new HttpEnvironment(),
            new TemplateHttpVariableResolver());

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(handler.SentRequests[0].RequestUri!.ToString()).IsEqualTo("https://catalog.example/items");
    }

    [Test]
    public async Task SendAsync_document_selects_client_by_client_name()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<CatalogApi>(client =>
            client.BaseAddress = new Uri("https://catalog.example/"));

        await using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var document = new HttpRequestDocument
        {
            Method = "GET",
            Uri = "https://catalog.example/items",
            ClientName = new CatalogApi().GetClientName(),
        };

        using var response = await factory.SendAsync(
            document,
            new HttpEnvironment(),
            new TemplateHttpVariableResolver());

        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(handler.SentRequests.Count).IsEqualTo(1);
    }

    public sealed class CatalogApi() : HttpClientKey;
}
