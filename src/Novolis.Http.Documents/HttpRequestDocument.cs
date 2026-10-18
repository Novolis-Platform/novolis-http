using System.Net.Http.Headers;
using System.Text;

namespace Novolis.Http.Documents;

/// <summary>Persistable, presentation-free description of an HTTP request.</summary>
public sealed record HttpRequestDocument
{
    /// <summary>Stable request identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Optional display name.</summary>
    public string? Name { get; init; }

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }

    /// <summary>HTTP method name.</summary>
    public string Method { get; init; } = HttpMethod.Get.Method;

    /// <summary>Request URI before query parameters are appended.</summary>
    public string Uri { get; init; } = "https://localhost/";

    /// <summary>Request headers.</summary>
    public IDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Query parameters.</summary>
    public IDictionary<string, string> QueryParameters { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Optional request body.</summary>
    public string? Body { get; init; }

    /// <summary>Optional request content type.</summary>
    public string? ContentType { get; init; }

    /// <summary>Environment used to resolve variables and authentication.</summary>
    public string? EnvironmentName { get; init; }

    /// <summary>Factory name of the named client, equal to <c>GetClientName()</c> on the key.</summary>
    public string? ClientName { get; init; }

    /// <summary>Creates an <see cref="HttpRequestMessage"/> from this document.</summary>
    public HttpRequestMessage CreateRequest()
    {
        var builder = new UriBuilder(Uri);
        if (QueryParameters.Count > 0)
        {
            var query = string.Join(
                "&",
                QueryParameters.Select(pair =>
                    $"{System.Uri.EscapeDataString(pair.Key)}={System.Uri.EscapeDataString(pair.Value)}"));
            builder.Query = string.IsNullOrWhiteSpace(builder.Query)
                ? query
                : $"{builder.Query.TrimStart('?')}&{query}";
        }

        var request = new HttpRequestMessage(new HttpMethod(Method), builder.Uri);
        foreach (var header in Headers)
        {
            if (IsCredentialHeader(header.Key))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                request.Content ??= new StringContent(string.Empty);
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (Body is not null)
        {
            request.Content = new StringContent(Body, Encoding.UTF8);
            if (!string.IsNullOrWhiteSpace(ContentType))
            {
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
            }
        }

        return request;
    }

    private static bool IsCredentialHeader(string name) =>
        name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
        || name.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase);
}
