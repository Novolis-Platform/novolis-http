using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Novolis.Http.Authentication.Bearer;

/// <summary>Applies a configured bearer token. The token is never refreshed.</summary>
internal sealed class BearerAuthenticationHandler(
    IOptionsMonitor<BearerAuthenticationConfiguration> options,
    string clientName) : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var settings = options.Get(clientName);
        if (string.IsNullOrWhiteSpace(settings.Token))
        {
            throw new InvalidOperationException("Token must be configured.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);
        return base.SendAsync(request, cancellationToken);
    }
}
