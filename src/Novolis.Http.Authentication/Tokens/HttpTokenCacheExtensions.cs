using Novolis.Http.Client;

namespace Novolis.Http.Authentication.Tokens;

/// <summary>Typed helpers for <see cref="IHttpTokenCache"/>.</summary>
public static class HttpTokenCacheExtensions
{
    /// <summary>Removes every entry owned by <typeparamref name="TApi"/>.</summary>
    /// <typeparam name="TApi">Client key.</typeparam>
    /// <param name="cache">Token cache.</param>
    public static void Invalidate<TApi>(this IHttpTokenCache cache)
        where TApi : HttpClientKey, new()
    {
        ArgumentNullException.ThrowIfNull(cache);
        cache.Invalidate(HttpClientKey.For<TApi>());
    }
}
