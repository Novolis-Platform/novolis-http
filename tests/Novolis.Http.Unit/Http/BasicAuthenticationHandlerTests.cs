using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Novolis.Http.Authentication.Basic;
using Novolis.Http.Client;
using Novolis.Http.Unit.Infrastructure;

namespace Novolis.Http.Unit;

public sealed class BasicAuthenticationHandlerTests
{
    [Test]
    public async Task SendAsync_sets_basic_authorization_header()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<CatalogApi>()
            .AddBasicAuthentication(options =>
            {
                options.Username = "user";
                options.Password = "secret";
            });

        await using var provider = services.BuildServiceProvider();
        using var response = await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<CatalogApi>()
            .GetAsync("https://catalog.example/");

        var expected = Convert.ToBase64String(Encoding.ASCII.GetBytes("user:secret"));
        var request = handler.SentRequests[0];
        await Assert.That(request.Headers.Authorization!.Scheme).IsEqualTo("Basic");
        await Assert.That(request.Headers.Authorization.Parameter).IsEqualTo(expected);
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task SendAsync_encodes_special_characters()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<CatalogApi>()
            .AddBasicAuthentication(options =>
            {
                options.Username = "a:b";
                options.Password = "c@d";
            });

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<CatalogApi>()
            .GetAsync("https://catalog.example/");

        var decoded = Encoding.ASCII.GetString(
            Convert.FromBase64String(handler.SentRequests[0].Headers.Authorization!.Parameter!));
        await Assert.That(decoded).IsEqualTo("a:b:c@d");
    }

    public sealed class CatalogApi() : HttpClientKey;
}
