using System.Text.RegularExpressions;
using Novolis.Http.Documents;

namespace Novolis.Http.Variables;

/// <summary>Resolves <c>{{name}}</c> placeholders from a scoped environment.</summary>
public sealed partial class TemplateHttpVariableResolver : IHttpVariableResolver
{
    /// <inheritdoc />
    public ValueTask<HttpRequestDocument> ResolveAsync(
        HttpRequestDocument document,
        HttpEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var variables = environment.Variables;
        return ValueTask.FromResult(document with
        {
            Uri = ResolveRequired(document.Uri, variables),
            Name = ResolveOptional(document.Name, variables),
            Description = ResolveOptional(document.Description, variables),
            Method = ResolveRequired(document.Method, variables),
            Body = ResolveOptional(document.Body, variables),
            ContentType = ResolveOptional(document.ContentType, variables),
            EnvironmentName = ResolveOptional(document.EnvironmentName, variables),
            ClientName = ResolveOptional(document.ClientName, variables),
            Headers = document.Headers.ToDictionary(
                pair => ResolveRequired(pair.Key, variables),
                pair => ResolveRequired(pair.Value, variables),
                StringComparer.OrdinalIgnoreCase),
            QueryParameters = document.QueryParameters.ToDictionary(
                pair => ResolveRequired(pair.Key, variables),
                pair => ResolveRequired(pair.Value, variables),
                StringComparer.OrdinalIgnoreCase),
        });
    }

    private static string? ResolveOptional(
        string? value,
        IDictionary<string, string> variables)
    {
        if (value is null)
        {
            return null;
        }

        return ResolveRequired(value, variables);
    }

    private static string ResolveRequired(
        string value,
        IDictionary<string, string> variables)
    {
        return Placeholder().Replace(value, match =>
            variables.TryGetValue(match.Groups["name"].Value, out var resolved)
                ? resolved
                : match.Value);
    }

    [GeneratedRegex(@"\{\{(?<name>[A-Za-z0-9_.-]+)\}\}")]
    private static partial Regex Placeholder();
}
