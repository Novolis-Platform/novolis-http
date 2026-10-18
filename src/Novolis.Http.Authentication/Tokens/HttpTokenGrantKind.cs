namespace Novolis.Http.Authentication.Tokens;

/// <summary>OAuth grant used to acquire an access token.</summary>
internal enum HttpTokenGrantKind
{
    /// <summary>Client credentials grant.</summary>
    ClientCredentials = 0,

    /// <summary>Refresh token grant.</summary>
    RefreshToken = 1,
}
