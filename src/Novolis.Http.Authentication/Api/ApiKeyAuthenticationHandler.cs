using Microsoft.Extensions.Options;

namespace Novolis.Http.Authentication.Api;

/// <summary>Applies an API key from named options.</summary>
internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationConfiguration> options,
    string clientName) : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var settings = options.Get(clientName);
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("ApiKey must be configured.");
        }

        if (!string.IsNullOrWhiteSpace(settings.QueryParameterName))
        {
            request.RequestUri = AppendQuery(request.RequestUri, settings.QueryParameterName, settings.ApiKey);
            return base.SendAsync(request, cancellationToken);
        }

        var headerName = string.IsNullOrWhiteSpace(settings.HeaderName) ? "X-Api-Key" : settings.HeaderName;
        var value = string.IsNullOrWhiteSpace(settings.Scheme)
            ? settings.ApiKey
            : $"{settings.Scheme} {settings.ApiKey}";
        request.Headers.Remove(headerName);
        request.Headers.TryAddWithoutValidation(headerName, value);
        return base.SendAsync(request, cancellationToken);
    }

    private static Uri AppendQuery(Uri? uri, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(uri);
        var pair = $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
        var text = uri.OriginalString;
        var separator = text.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return new Uri($"{text}{separator}{pair}", uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
    }
}
