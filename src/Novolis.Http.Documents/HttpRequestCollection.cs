namespace Novolis.Http.Documents;

/// <summary>Persisted collection of HTTP request documents and environments.</summary>
public sealed record HttpRequestCollection
{
    /// <summary>Document format version.</summary>
    public int Version { get; init; } = 1;

    /// <summary>Saved environments.</summary>
    public IList<HttpEnvironment> Environments { get; init; } = [];

    /// <summary>Saved request documents.</summary>
    public IList<HttpRequestDocument> Requests { get; init; } = [];
}
