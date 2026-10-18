using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace Novolis.Http.Authentication.Basic;

/// <summary>Applies HTTP Basic authentication from named options.</summary>
internal sealed class BasicAuthenticationHandler(
    IOptionsMonitor<BasicAuthenticationConfiguration> options,
    string clientName) : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var settings = options.Get(clientName);
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return base.SendAsync(request, cancellationToken);
    }
}
