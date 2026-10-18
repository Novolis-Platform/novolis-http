using Novolis.Http.Documents;

namespace Novolis.Http.Client;

/// <summary>Sends persistable request documents through <see cref="HttpClient"/>.</summary>
public static class HttpClientExtensions
{
    /// <summary>Creates an <see cref="HttpRequestMessage"/> from <paramref name="document"/> and sends it.</summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="document">Request document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    public static async Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        HttpRequestDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(document);
        using var request = document.CreateRequest();
        return await client.SendAsync(request, cancellationToken);
    }
}
