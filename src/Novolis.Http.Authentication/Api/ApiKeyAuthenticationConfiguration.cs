namespace Novolis.Http.Authentication.Api;

/// <summary>API key header or query configuration.</summary>
public sealed class ApiKeyAuthenticationConfiguration
{
    /// <summary>Header name for the API key when a query parameter is not set.</summary>
    public string HeaderName { get; set; } = "X-Api-Key";

    /// <summary>API key value sent on each request.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// When set, the key is sent as this query parameter instead of a header.
    /// Query values are copied into access logs, proxies, and Referer. Prefer the header default.
    /// </summary>
    public string? QueryParameterName { get; set; }

    /// <summary>Optional header scheme prefix such as <c>Bearer</c> or <c>ApiKey</c>.</summary>
    public string? Scheme { get; set; }
}
