using System.Collections.Concurrent;

namespace Novolis.Http.Client;

/// <summary>Caches <see cref="HttpClientKey.GetClientName()"/> for each closed key type.</summary>
internal static class HttpClientKeyNames
{
    private static readonly ConcurrentDictionary<Type, string> Names = new();

    /// <summary>Returns the factory name for <typeparamref name="TApi"/>, constructing the key once.</summary>
    public static string For<TApi>()
        where TApi : HttpClientKey, new() =>
        Names.GetOrAdd(typeof(TApi), static _ => new TApi().GetClientName());
}
