namespace Novolis.Http.Documents;

/// <summary>A named scope of variables and authentication selection for HTTP documents.</summary>
public sealed record HttpEnvironment
{
    /// <summary>Stable environment identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Display name.</summary>
    public string Name { get; init; } = "Default";

    /// <summary>Variables available to request documents.</summary>
    public IDictionary<string, string> Variables { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Factory name of the named client selected by this environment.</summary>
    public string? ClientName { get; init; }

    /// <summary>Parent environment used for inherited variables.</summary>
    public Guid? ParentEnvironmentId { get; init; }
}
