using Microsoft.Extensions.DependencyInjection;
using Novolis.Http.Authentication.Bearer;
using Novolis.Http.Client;
using Novolis.Http.Unit.Infrastructure;

namespace Novolis.Http.Unit;

public sealed class BearerAuthenticationHandlerTests
{
    [Test]
    public async Task SendAsync_sets_static_bearer_token()
    {
        var handler = new StubHttpMessageHandler();
        var services = new ServiceCollection();
        services.ConfigureTestHttpHandler(handler);
        services.AddNovolisHttp();
        services.AddHttpClientFor<GitApi>()
            .AddBearerAuthentication(options => options.Token = "github_pat_test");

        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient<GitApi>()
            .GetAsync("https://api.github.com/user");

        var authorization = handler.SentRequests[0].Headers.Authorization;
        await Assert.That(authorization).IsNotNull();
        await Assert.That(authorization!.Scheme).IsEqualTo("Bearer");
        await Assert.That(authorization.Parameter).IsEqualTo("github_pat_test");
    }

    public sealed class GitApi() : HttpClientKey;
}
