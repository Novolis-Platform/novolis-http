using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Novolis.Http.Authentication.Tokens;

namespace Novolis.Http.Authentication.OAuth;

/// <summary>Registers refresh-token authentication on a named HTTP client.</summary>
public static class RefreshTokenAuthenticationHttpClientBuilderExtensions
{
    /// <summary>Adds bearer authentication that refreshes when the access token expires.</summary>
    /// <param name="builder">HTTP client builder.</param>
    /// <param name="configure">Grant configuration.</param>
    /// <returns>The client builder.</returns>
    public static IHttpClientBuilder AddRefreshTokenAuthentication(
        this IHttpClientBuilder builder,
        Action<RefreshTokenAuthenticationConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);
        builder.Services.AddHttpTokenServices();
        builder.Services.AddOptions<RefreshTokenAuthenticationConfiguration>(builder.Name).Configure(configure);
        builder.AddHttpMessageHandler(provider =>
            new RefreshTokenAuthenticationHandler(
                provider.GetRequiredService<IOptionsMonitor<RefreshTokenAuthenticationConfiguration>>(),
                provider.GetRequiredService<HttpTokenAcquirer>(),
                provider.GetRequiredService<IHttpTokenCache>(),
                builder.Name));
        return builder;
    }
}
