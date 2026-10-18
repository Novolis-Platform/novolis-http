using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Novolis.Http.Authentication.Basic;

/// <summary>Registers Basic authentication on a named HTTP client.</summary>
public static class BasicAuthenticationHttpClientBuilderExtensions
{
    /// <summary>Adds <c>Authorization: Basic</c> from named options.</summary>
    /// <param name="builder">HTTP client builder.</param>
    /// <param name="configure">Credential configuration.</param>
    /// <returns>The client builder.</returns>
    public static IHttpClientBuilder AddBasicAuthentication(
        this IHttpClientBuilder builder,
        Action<BasicAuthenticationConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);
        builder.Services.AddOptions<BasicAuthenticationConfiguration>(builder.Name).Configure(configure);
        builder.AddHttpMessageHandler(provider =>
            new BasicAuthenticationHandler(
                provider.GetRequiredService<IOptionsMonitor<BasicAuthenticationConfiguration>>(),
                builder.Name));
        return builder;
    }
}
