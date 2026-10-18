namespace Novolis.Http.Authentication.Tokens;

/// <summary>Inputs for a token endpoint call.</summary>
internal sealed class HttpTokenAcquisitionRequest
{
    /// <summary>Named client that owns the token.</summary>
    public required string ClientName { get; init; }

    /// <summary>Absolute token endpoint.</summary>
    public required string TokenEndpoint { get; init; }

    /// <summary>OAuth client id.</summary>
    public required string ClientId { get; init; }

    /// <summary>OAuth client secret when required.</summary>
    public string? ClientSecret { get; init; }

    /// <summary>Requested scope.</summary>
    public string? Scope { get; init; }

    /// <summary>Grant to use when the cache is empty or expired.</summary>
    public required HttpTokenGrantKind Grant { get; init; }

    /// <summary>Refresh token from options when the cache has none.</summary>
    public string? RefreshToken { get; init; }

    /// <summary>Optional seed access token.</summary>
    public string? SeedAccessToken { get; init; }

    /// <summary>Optional seed lifetime in seconds.</summary>
    public int? SeedExpiresIn { get; init; }
}
