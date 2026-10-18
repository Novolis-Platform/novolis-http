namespace Novolis.Http.Client;

/// <summary>Base type for a named <see cref="HttpClient"/>. Derived types add no members.</summary>
public class HttpClientKey
{
    /// <summary>Creates a key. The factory name is the derived type's full name.</summary>
    /// <exception cref="InvalidOperationException">The runtime type is the base, abstract, or generic.</exception>
    protected HttpClientKey()
    {
        EnsureValidKeyType(GetType());
    }

    /// <summary>Returns the normalized full name of this key type.</summary>
    /// <returns>The stable factory name.</returns>
    /// <exception cref="InvalidOperationException">The runtime type is the base, abstract, or generic.</exception>
    public string GetClientName() => GetClientName(GetType());

    /// <summary>Returns the normalized full name of <paramref name="type"/>.</summary>
    /// <param name="type">Client key type.</param>
    /// <returns>The stable factory name.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="type"/> is the base, abstract, generic, or has no full name.</exception>
    public static string GetClientName(Type type)
    {
        EnsureValidKeyType(type);
        return Normalize(type);
    }

    /// <summary>Throws when <paramref name="type"/> cannot be used as a client key.</summary>
    /// <param name="type">Client key type.</param>
    public static void EnsureValidKeyType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (type == typeof(HttpClientKey))
        {
            throw new InvalidOperationException("HttpClientKey cannot be used as a client key.");
        }

        if (type.IsAbstract)
        {
            throw new InvalidOperationException($"{type.FullName} is abstract and cannot be used as a client key.");
        }

        if (type.IsGenericType)
        {
            throw new InvalidOperationException($"{type.FullName} is generic and cannot be used as a client key.");
        }
    }

    /// <summary>Returns the cached factory name for <typeparamref name="TApi"/>.</summary>
    /// <typeparam name="TApi">Client key.</typeparam>
    /// <returns>The name from <see cref="GetClientName()"/>.</returns>
    public static string For<TApi>()
        where TApi : HttpClientKey, new() =>
        HttpClientKeyNames.For<TApi>();

    private static string Normalize(Type type)
    {
        var fullName = type.FullName;
        if (string.IsNullOrEmpty(fullName))
        {
            throw new InvalidOperationException($"{type.Name} has no full name and cannot be used as a client key.");
        }

        return fullName;
    }
}
