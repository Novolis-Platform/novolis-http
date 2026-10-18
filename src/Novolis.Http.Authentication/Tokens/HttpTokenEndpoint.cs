namespace Novolis.Http.Authentication.Tokens;

/// <summary>Accepts absolute HTTPS token endpoints, or HTTP when the host is loopback.</summary>
internal static class HttpTokenEndpoint
{
    /// <summary>Throws when <paramref name="tokenEndpoint"/> is not an allowed token URL.</summary>
    public static void EnsureAllowed(string tokenEndpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenEndpoint);
        if (!Uri.TryCreate(tokenEndpoint, UriKind.Absolute, out var uri) || !IsAllowed(uri))
        {
            throw new InvalidOperationException(
                "TokenEndpoint must be an absolute https URI. Loopback http is allowed.");
        }
    }

    /// <summary>Returns whether <paramref name="uri"/> may receive a token POST.</summary>
    public static bool IsAllowed(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri)
        {
            return false;
        }

        if (string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && uri.IsLoopback;
    }
}
