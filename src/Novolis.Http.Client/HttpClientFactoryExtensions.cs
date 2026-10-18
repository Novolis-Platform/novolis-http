using Microsoft.Extensions.Options;
using Novolis.Http.Documents;
using Novolis.Http.Variables;

namespace Novolis.Http.Client;

/// <summary>Resolves typed and document-selected clients from <see cref="IHttpClientFactory"/>.</summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>Creates the <see cref="HttpClient"/> registered for <typeparamref name="TApi"/>.</summary>
    /// <typeparam name="TApi">Client key.</typeparam>
    /// <param name="factory">HTTP client factory.</param>
    /// <returns>The named client.</returns>
    public static HttpClient CreateClient<TApi>(this IHttpClientFactory factory)
        where TApi : HttpClientKey, new()
    {
        ArgumentNullException.ThrowIfNull(factory);
        return factory.CreateClient(HttpClientKeyNames.For<TApi>());
    }

    /// <summary>Resolves variables and sends the document through <typeparamref name="TApi"/>.</summary>
    /// <typeparam name="TApi">Client key.</typeparam>
    /// <param name="factory">HTTP client factory.</param>
    /// <param name="document">Request document.</param>
    /// <param name="environment">Variable environment.</param>
    /// <param name="resolver">Variable resolver.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    public static async Task<HttpResponseMessage> SendAsync<TApi>(
        this IHttpClientFactory factory,
        HttpRequestDocument document,
        HttpEnvironment environment,
        IHttpVariableResolver resolver,
        CancellationToken cancellationToken = default)
        where TApi : HttpClientKey, new()
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(resolver);
        var resolved = await resolver.ResolveAsync(document, environment, cancellationToken);
        return await factory.CreateClient<TApi>().SendAsync(resolved, cancellationToken);
    }

    /// <summary>Resolves variables and sends through the client named by the document or environment.</summary>
    /// <param name="factory">HTTP client factory.</param>
    /// <param name="document">Request document.</param>
    /// <param name="environment">Variable environment.</param>
    /// <param name="resolver">Variable resolver.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    public static async Task<HttpResponseMessage> SendAsync(
        this IHttpClientFactory factory,
        HttpRequestDocument document,
        HttpEnvironment environment,
        IHttpVariableResolver resolver,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(resolver);
        var resolved = await resolver.ResolveAsync(document, environment, cancellationToken);
        var name = resolved.ClientName ?? environment.ClientName ?? Options.DefaultName;
        return await factory.CreateClient(name).SendAsync(resolved, cancellationToken);
    }
}
