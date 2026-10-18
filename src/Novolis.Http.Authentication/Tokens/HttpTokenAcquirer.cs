using System.Collections.Concurrent;
using System.Text.Json;

namespace Novolis.Http.Authentication.Tokens;

/// <summary>Acquires and caches OAuth access tokens through the token client.</summary>
internal sealed class HttpTokenAcquirer(
    IHttpClientFactory httpClientFactory,
    IHttpTokenCache cache,
    TimeProvider time)
{
    private readonly ConcurrentDictionary<string, Lazy<Task<CachedHttpToken>>> _inflight = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _seeded = new(StringComparer.Ordinal);

    /// <summary>Returns a valid access token, acquiring or refreshing when needed.</summary>
    public async Task<CachedHttpToken> GetTokenAsync(
        HttpTokenAcquisitionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var cacheKey = HttpTokenCacheKey.Create(
            request.ClientName,
            request.TokenEndpoint,
            request.ClientId,
            request.Scope);
        if (cache.TryGet(cacheKey, out var existing) && existing is not null)
        {
            return existing;
        }

        if (_seeded.TryAdd(cacheKey, 0))
        {
            Seed(request, cacheKey);
        }

        if (cache.TryGet(cacheKey, out existing) && existing is not null)
        {
            return existing;
        }

        var lazy = _inflight.GetOrAdd(
            cacheKey,
            key => new Lazy<Task<CachedHttpToken>>(() => AcquireAsync(request, key, CancellationToken.None)));
        try
        {
            var token = await lazy.Value;
            cancellationToken.ThrowIfCancellationRequested();
            return token;
        }
        finally
        {
            _inflight.TryRemove(new KeyValuePair<string, Lazy<Task<CachedHttpToken>>>(cacheKey, lazy));
        }
    }

    private void Seed(HttpTokenAcquisitionRequest request, string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(request.SeedAccessToken))
        {
            return;
        }

        var lifetime = HttpTokenLifetime.FromExpiresIn(request.SeedExpiresIn);
        cache.Set(
            request.ClientName,
            cacheKey,
            new CachedHttpToken
            {
                AccessToken = request.SeedAccessToken,
                ExpiresAt = time.GetUtcNow().Add(lifetime),
                RefreshToken = request.RefreshToken,
            });
    }

    private async Task<CachedHttpToken> AcquireAsync(
        HttpTokenAcquisitionRequest request,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TokenEndpoint))
        {
            throw new InvalidOperationException("TokenEndpoint must be configured.");
        }

        HttpTokenEndpoint.EnsureAllowed(request.TokenEndpoint);

        cache.TryGet(cacheKey, out var previous, includeExpired: true);
        var refreshToken = previous?.RefreshToken ?? request.RefreshToken;
        var useRefresh = request.Grant == HttpTokenGrantKind.RefreshToken;
        if (useRefresh && string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new InvalidOperationException("A refresh token is required.");
        }

        var form = useRefresh
            ? RefreshForm(request, refreshToken!)
            : ClientCredentialsForm(request);
        using var message = new HttpRequestMessage(HttpMethod.Post, request.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(form),
        };
        var client = httpClientFactory.CreateClient(HttpTokenClient.Name);
        using var response = await client.SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        OAuthTokenResponse tokenResponse;
        try
        {
            tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(body)
                ?? throw new InvalidOperationException("Token endpoint returned an empty or invalid JSON body.");
        }
        catch (JsonException) when (!response.IsSuccessStatusCode)
        {
            cache.Invalidate(request.ClientName);
            response.EnsureSuccessStatusCode();
            throw;
        }

        if (string.Equals(tokenResponse.Error, "invalid_grant", StringComparison.OrdinalIgnoreCase))
        {
            cache.Invalidate(request.ClientName);
            throw new InvalidOperationException("Token endpoint returned invalid_grant.");
        }

        if (!response.IsSuccessStatusCode)
        {
            cache.Invalidate(request.ClientName);
            response.EnsureSuccessStatusCode();
        }

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Token endpoint returned an empty access_token.");
        }

        var stored = new CachedHttpToken
        {
            AccessToken = tokenResponse.AccessToken,
            TokenType = string.IsNullOrWhiteSpace(tokenResponse.TokenType) ? "Bearer" : tokenResponse.TokenType,
            ExpiresAt = time.GetUtcNow().Add(HttpTokenLifetime.FromExpiresIn(tokenResponse.ExpiresIn)),
            RefreshToken = tokenResponse.RefreshToken ?? (useRefresh ? refreshToken : null),
        };
        cache.Set(request.ClientName, cacheKey, stored);
        return stored;
    }

    private static Dictionary<string, string> ClientCredentialsForm(HttpTokenAcquisitionRequest request)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = request.ClientId,
        };
        if (!string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            form["client_secret"] = request.ClientSecret;
        }

        if (!string.IsNullOrWhiteSpace(request.Scope))
        {
            form["scope"] = request.Scope;
        }

        return form;
    }

    private static Dictionary<string, string> RefreshForm(
        HttpTokenAcquisitionRequest request,
        string refreshToken)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = request.ClientId,
        };
        if (!string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            form["client_secret"] = request.ClientSecret;
        }

        if (!string.IsNullOrWhiteSpace(request.Scope))
        {
            form["scope"] = request.Scope;
        }

        return form;
    }
}
