namespace Novolis.Http.Authentication.Basic;

/// <summary>Username and password for HTTP Basic authentication.</summary>
public sealed class BasicAuthenticationConfiguration
{
    /// <summary>Basic auth username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Basic auth password.</summary>
    public string Password { get; set; } = string.Empty;
}
