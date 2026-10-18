using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Novolis.Http.Authentication.Api;

/// <summary>Registers API key authentication on a named HTTP client.</summary>
public static class ApiKeyAuthenticationHttpClientBuilderExtensions
{
    /// <summary>Adds an API key header or query parameter from named options.</summary>
    /// <param name="builder">HTTP client builder.</param>
    /// <param name="configure">API key configuration.</param>
    /// <returns>The client builder.</returns>
    public static IHttpClientBuilder AddApiKeyAuthentication(
        this IHttpClientBuilder builder,
        Action<ApiKeyAuthenticationConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);
        builder.Services.AddOptions<ApiKeyAuthenticationConfiguration>(builder.Name).Configure(configure);
        builder.AddHttpMessageHandler(provider =>
            new ApiKeyAuthenticationHandler(
                provider.GetRequiredService<IOptionsMonitor<ApiKeyAuthenticationConfiguration>>(),
                builder.Name));
        return builder;
    }
}
