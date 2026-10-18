using System.Net;
using System.Net.Http.Headers;

namespace Novolis.Http.Authentication.Tokens;

/// <summary>Sends a resource request with a cached token and one 401 retry.</summary>
internal static class OAuthResourceSend
{
    /// <summary>Applies the token, sends, and retries once after a 401.</summary>
    public static async Task<HttpResponseMessage> SendAsync(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send,
        HttpRequestMessage request,
        HttpTokenAcquirer acquirer,
        IHttpTokenCache cache,
        HttpTokenAcquisitionRequest acquisition,
        CancellationToken cancellationToken)
    {
        var body = await HttpRequestMessageCopy.ReadBodyAsync(request, cancellationToken);
        var first = await SendOnceAsync(send, request, body, acquirer, acquisition, cancellationToken);
        if (first.StatusCode != HttpStatusCode.Unauthorized)
        {
            return first;
        }

        first.Dispose();
        cache.ExpireAccessToken(
            HttpTokenCacheKey.Create(
                acquisition.ClientName,
                acquisition.TokenEndpoint,
                acquisition.ClientId,
                acquisition.Scope));
        return await SendOnceAsync(send, request, body, acquirer, acquisition, cancellationToken);
    }

    private static async Task<HttpResponseMessage> SendOnceAsync(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send,
        HttpRequestMessage request,
        byte[]? body,
        HttpTokenAcquirer acquirer,
        HttpTokenAcquisitionRequest acquisition,
        CancellationToken cancellationToken)
    {
        var token = await acquirer.GetTokenAsync(acquisition, cancellationToken);
        var outgoing = HttpRequestMessageCopy.Copy(request, body, cancellationToken);
        outgoing.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
        return await send(outgoing, cancellationToken);
    }
}
