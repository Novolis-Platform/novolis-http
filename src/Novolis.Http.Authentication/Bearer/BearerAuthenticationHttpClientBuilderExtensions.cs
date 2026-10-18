using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Novolis.Http.Authentication.Bearer;

/// <summary>Registers static bearer authentication on a named HTTP client.</summary>
public static class BearerAuthenticationHttpClientBuilderExtensions
{
    /// <summary>Adds <c>Authorization: Bearer</c> from a configured token.</summary>
    /// <param name="builder">HTTP client builder.</param>
    /// <param name="configure">Token configuration.</param>
    /// <returns>The client builder.</returns>
    public static IHttpClientBuilder AddBearerAuthentication(
        this IHttpClientBuilder builder,
        Action<BearerAuthenticationConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);
        builder.Services.AddOptions<BearerAuthenticationConfiguration>(builder.Name).Configure(configure);
        builder.AddHttpMessageHandler(provider =>
            new BearerAuthenticationHandler(
                provider.GetRequiredService<IOptionsMonitor<BearerAuthenticationConfiguration>>(),
                builder.Name));
        return builder;
    }
}
