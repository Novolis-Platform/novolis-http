using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Novolis.Http.Authentication.Tokens;

/// <summary>Stores tokens in <see cref="IMemoryCache"/> and tracks keys per client name.</summary>
public sealed class MemoryHttpTokenCache(IMemoryCache cache, TimeProvider time) : IHttpTokenCache
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _keysByClient = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public bool TryGet(string cacheKey, out CachedHttpToken? token, bool includeExpired = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);
        if (cache.TryGetValue(cacheKey, out CachedHttpToken? stored) && stored is not null)
        {
            if (includeExpired || stored.ExpiresAt > time.GetUtcNow())
            {
                token = stored;
                return true;
            }
        }

        token = null;
        return false;
    }

    /// <inheritdoc />
    public void Set(string clientName, string cacheKey, CachedHttpToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);
        ArgumentNullException.ThrowIfNull(token);
        var entry = new MemoryCacheEntryOptions();
        if (string.IsNullOrEmpty(token.RefreshToken))
        {
            entry.AbsoluteExpiration = token.ExpiresAt;
        }

        cache.Set(cacheKey, token, entry);
        _keysByClient.GetOrAdd(clientName, static _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal))
            .TryAdd(cacheKey, 0);
    }

    /// <inheritdoc />
    public void ExpireAccessToken(string cacheKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheKey);
        if (!cache.TryGetValue(cacheKey, out CachedHttpToken? stored) || stored is null)
        {
            return;
        }

        var expired = new CachedHttpToken
        {
            AccessToken = stored.AccessToken,
            TokenType = stored.TokenType,
            ExpiresAt = time.GetUtcNow().AddTicks(-1),
            RefreshToken = stored.RefreshToken,
        };
        var entry = new MemoryCacheEntryOptions();
        if (string.IsNullOrEmpty(expired.RefreshToken))
        {
            entry.AbsoluteExpiration = time.GetUtcNow();
        }

        cache.Set(cacheKey, expired, entry);
    }

    /// <inheritdoc />
    public void Invalidate(string clientName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        if (!_keysByClient.TryRemove(clientName, out var keys))
        {
            return;
        }

        foreach (var key in keys.Keys)
        {
            cache.Remove(key);
        }
    }
}
