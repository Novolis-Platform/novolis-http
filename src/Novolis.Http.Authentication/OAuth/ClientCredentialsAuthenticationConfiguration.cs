namespace Novolis.Http.Authentication.OAuth;

/// <summary>Client-credentials settings for token acquisition.</summary>
public sealed class ClientCredentialsAuthenticationConfiguration
{
    /// <summary>OAuth client id.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>OAuth client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Requested scope.</summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>Absolute token endpoint URL.</summary>
    public string TokenEndpoint { get; set; } = string.Empty;
}
