namespace Novolis.Http.Authentication.Bearer;

/// <summary>Static bearer token held by the host.</summary>
public sealed class BearerAuthenticationConfiguration
{
    /// <summary>Access token sent on every request.</summary>
    public string Token { get; set; } = string.Empty;
}
