namespace Novolis.Http.Authentication.OAuth;

/// <summary>Refresh-token settings for token acquisition.</summary>
public sealed class RefreshTokenAuthenticationConfiguration
{
    /// <summary>OAuth client id.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Optional OAuth client secret.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Optional requested scope.</summary>
    public string? Scope { get; set; }

    /// <summary>Absolute token endpoint URL.</summary>
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>Refresh token used when the access token is missing or expired.</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Optional seed access token.</summary>
    public string? AccessToken { get; set; }

    /// <summary>Optional seed lifetime in seconds.</summary>
    public int? ExpiresIn { get; set; }
}
