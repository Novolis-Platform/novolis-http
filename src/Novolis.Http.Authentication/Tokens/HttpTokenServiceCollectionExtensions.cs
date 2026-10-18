using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Novolis.Http.Authentication.Tokens;

/// <summary>Registers the token cache, acquirer, and token HTTP client.</summary>
internal static class HttpTokenServiceCollectionExtensions
{
    /// <summary>Adds shared token services when a token grant helper is used.</summary>
    public static void AddHttpTokenServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddMemoryCache();
        services.TryAddSingleton<IHttpTokenCache, MemoryHttpTokenCache>();
        services.TryAddSingleton<HttpTokenAcquirer>();
        services.AddHttpClient(HttpTokenClient.Name);
    }
}
