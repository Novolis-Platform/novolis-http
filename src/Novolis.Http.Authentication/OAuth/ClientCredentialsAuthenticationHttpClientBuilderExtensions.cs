using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Novolis.Http.Authentication.Tokens;

namespace Novolis.Http.Authentication.OAuth;

/// <summary>Registers client-credentials authentication on a named HTTP client.</summary>
public static class ClientCredentialsAuthenticationHttpClientBuilderExtensions
{
    /// <summary>Adds bearer authentication using the client-credentials grant.</summary>
    /// <param name="builder">HTTP client builder.</param>
    /// <param name="configure">Grant configuration.</param>
    /// <returns>The client builder.</returns>
    public static IHttpClientBuilder AddClientCredentialsAuthentication(
        this IHttpClientBuilder builder,
        Action<ClientCredentialsAuthenticationConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);
        builder.Services.AddHttpTokenServices();
        builder.Services.AddOptions<ClientCredentialsAuthenticationConfiguration>(builder.Name).Configure(configure);
        builder.AddHttpMessageHandler(provider =>
            new ClientCredentialsAuthenticationHandler(
                provider.GetRequiredService<IOptionsMonitor<ClientCredentialsAuthenticationConfiguration>>(),
                provider.GetRequiredService<HttpTokenAcquirer>(),
                provider.GetRequiredService<IHttpTokenCache>(),
                builder.Name));
        return builder;
    }
}
