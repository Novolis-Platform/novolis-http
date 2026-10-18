using Novolis.Http.Documents;
using Novolis.Http.Variables;

namespace Novolis.Http.Unit;

public sealed class HttpDocumentTests
{
    [Test]
    public async Task Variable_resolver_replaces_uri_headers_and_body()
    {
        var document = new HttpRequestDocument
        {
            Uri = "https://{{host}}/items",
            Headers = new Dictionary<string, string> { ["X-Trace"] = "{{trace}}" },
            Body = """{"name":"{{name}}"}""",
        };
        var environment = new HttpEnvironment
        {
            Variables = new Dictionary<string, string>
            {
                ["host"] = "api.test",
                ["trace"] = "abc",
                ["name"] = "novolis",
            },
        };

        var resolved = await new TemplateHttpVariableResolver()
            .ResolveAsync(document, environment);

        await Assert.That(resolved.Uri).IsEqualTo("https://api.test/items");
        await Assert.That(resolved.Headers["X-Trace"]).IsEqualTo("abc");
        await Assert.That(resolved.Body).IsEqualTo("""{"name":"novolis"}""");
    }

    [Test]
    public async Task Request_document_builds_query_and_content()
    {
        var document = new HttpRequestDocument
        {
            Method = "POST",
            Uri = "https://api.test/items",
            QueryParameters = new Dictionary<string, string> { ["page"] = "1" },
            Body = """{"value":1}""",
            ContentType = "application/json",
        };

        using var request = document.CreateRequest();

        await Assert.That(request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(request.RequestUri!.Query).IsEqualTo("?page=1");
        await Assert.That(await request.Content!.ReadAsStringAsync()).IsEqualTo("""{"value":1}""");
    }

    [Test]
    public async Task CreateRequest_drops_authorization_headers()
    {
        var document = new HttpRequestDocument
        {
            Uri = "https://api.test/items",
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer secret",
                ["Proxy-Authorization"] = "Basic abc",
                ["X-Trace"] = "ok",
            },
        };

        using var request = document.CreateRequest();

        await Assert.That(request.Headers.Authorization).IsNull();
        await Assert.That(request.Headers.Contains("Proxy-Authorization")).IsFalse();
        await Assert.That(request.Headers.GetValues("X-Trace").Single()).IsEqualTo("ok");
    }
}
