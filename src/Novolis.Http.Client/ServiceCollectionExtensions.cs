using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Http.Client;

/// <summary>Registers Novolis HTTP clients on <see cref="IHttpClientFactory"/>.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the default HTTP client factory.</summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNovolisHttp(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpClient();
        return services;
    }

    /// <summary>Registers a named <see cref="HttpClient"/> for <typeparamref name="TApi"/>.</summary>
    /// <typeparam name="TApi">Client key. Not registered in dependency injection.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional client configuration.</param>
    /// <returns>The client builder for handlers and authentication.</returns>
    public static IHttpClientBuilder AddHttpClientFor<TApi>(
        this IServiceCollection services,
        Action<HttpClient>? configure = null)
        where TApi : HttpClientKey, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        var builder = services.AddHttpClient(HttpClientKeyNames.For<TApi>());
        if (configure is not null)
        {
            builder.ConfigureHttpClient(configure);
        }

        return builder;
    }
}
