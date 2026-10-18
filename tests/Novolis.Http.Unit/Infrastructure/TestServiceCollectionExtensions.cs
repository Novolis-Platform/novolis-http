using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Http.Unit.Infrastructure;

internal static class TestServiceCollectionExtensions
{
    /// <summary>
    /// Routes all <see cref="IHttpClientFactory"/>-created clients through <paramref name="handler"/>.
    /// Call before <c>AddNovolisHttp</c> (or any <c>AddHttpClient</c>) in tests.
    /// </summary>
    public static IServiceCollection ConfigureTestHttpHandler(this IServiceCollection services, StubHttpMessageHandler handler)
    {
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        return services;
    }
}
