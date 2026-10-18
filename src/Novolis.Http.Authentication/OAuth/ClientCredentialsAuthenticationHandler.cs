using Microsoft.Extensions.Options;
using Novolis.Http.Authentication.Tokens;

namespace Novolis.Http.Authentication.OAuth;

/// <summary>Acquires a client-credentials token and applies it to resource requests.</summary>
internal sealed class ClientCredentialsAuthenticationHandler(
    IOptionsMonitor<ClientCredentialsAuthenticationConfiguration> options,
    HttpTokenAcquirer acquirer,
    IHttpTokenCache cache,
    string clientName) : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var settings = options.Get(clientName);
        var acquisition = new HttpTokenAcquisitionRequest
        {
            ClientName = clientName,
            TokenEndpoint = settings.TokenEndpoint,
            ClientId = settings.ClientId,
            ClientSecret = settings.ClientSecret,
            Scope = settings.Scope,
            Grant = HttpTokenGrantKind.ClientCredentials,
        };
        return OAuthResourceSend.SendAsync(base.SendAsync, request, acquirer, cache, acquisition, cancellationToken);
    }
}
