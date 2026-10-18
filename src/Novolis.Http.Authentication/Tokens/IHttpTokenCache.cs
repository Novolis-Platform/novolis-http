namespace Novolis.Http.Authentication.Tokens;

/// <summary>In-memory cache of acquired access tokens.</summary>
public interface IHttpTokenCache
{
    /// <summary>Tries to read a token for <paramref name="cacheKey"/>.</summary>
    /// <param name="cacheKey">Token cache key.</param>
    /// <param name="token">The stored token when found.</param>
    /// <param name="includeExpired">When <see langword="true"/>, expired entries are returned so a refresh token can be reused.</param>
    /// <returns><see langword="true"/> when a token is present.</returns>
    bool TryGet(string cacheKey, out CachedHttpToken? token, bool includeExpired = false);

    /// <summary>Stores a token under <paramref name="cacheKey"/> for <paramref name="clientName"/>.</summary>
    /// <param name="clientName">Named client that owns the entry.</param>
    /// <param name="cacheKey">Token cache key.</param>
    /// <param name="token">Token to store.</param>
    void Set(string clientName, string cacheKey, CachedHttpToken token);

    /// <summary>Marks the access token expired and keeps any refresh token.</summary>
    /// <param name="cacheKey">Token cache key.</param>
    void ExpireAccessToken(string cacheKey);

    /// <summary>Removes every entry owned by <paramref name="clientName"/>.</summary>
    /// <param name="clientName">Named client.</param>
    void Invalidate(string clientName);
}
