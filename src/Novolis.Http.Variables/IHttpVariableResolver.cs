using Novolis.Http.Documents;

namespace Novolis.Http.Variables;

/// <summary>Resolves variables in a request document using an HTTP environment.</summary>
public interface IHttpVariableResolver
{
    /// <summary>Resolves a document without blocking on asynchronous providers.</summary>
    /// <param name="document">Document to resolve.</param>
    /// <param name="environment">Variable environment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A resolved document.</returns>
    ValueTask<HttpRequestDocument> ResolveAsync(
        HttpRequestDocument document,
        HttpEnvironment environment,
        CancellationToken cancellationToken = default);
}
