namespace Novolis.Http.Authentication.Tokens;

/// <summary>Access token stored for a named client.</summary>
public sealed class CachedHttpToken
{
    /// <summary>Access token value.</summary>
    public required string AccessToken { get; init; }

    /// <summary>Token type sent as the authorization scheme.</summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>Instant after which the token is treated as expired.</summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>Refresh token when the grant returned or stored one.</summary>
    public string? RefreshToken { get; init; }
}
