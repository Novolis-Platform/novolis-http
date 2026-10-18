namespace Novolis.Http.Authentication.Tokens;

/// <summary>Deterministic cache key for an access token.</summary>
internal sealed class HttpTokenCacheKey
{
    /// <summary>Builds the key from client name and token grant identity.</summary>
    public static string Create(string clientName, string tokenEndpoint, string clientId, string? scope) =>
        string.Join('|', clientName, tokenEndpoint, clientId, scope ?? string.Empty);
}
