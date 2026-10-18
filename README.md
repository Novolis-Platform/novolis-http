# novolis-http

Presentation-free HTTP documents, environments, variable resolution, and `IHttpClientFactory` execution for the Novolis platform.

## Packages

| Package | Purpose |
|---|---|
| `Novolis.Http.Client` | `HttpClientKey`, `AddHttpClientFor<TApi>`, and document send helpers |
| `Novolis.Http.Authentication` | Basic, API key, bearer, client-credentials, and refresh-token handlers |
| `Novolis.Http.Documents` | Persistable requests and scoped environments |
| `Novolis.Http.Variables` | Async-safe `{{name}}` substitution |

The request pipeline is:

```text
HttpRequestDocument → variable resolution → named HttpClient → DelegatingHandler authentication → response
```

The library has no WPF, Avalonia, Blazor, or MAUI dependency. GUI, CLI, test, and automation hosts can consume the same document model.

## Quick start

```csharp
public sealed class CatalogApi() : HttpClientKey;

services.AddNovolisHttp();
services.AddHttpClientFor<CatalogApi>(client =>
{
    client.BaseAddress = new Uri("https://catalog.example/");
})
.AddBasicAuthentication(options =>
{
    options.Username = "ada";
    options.Password = "secret";
});

HttpClient catalog = factory.CreateClient<CatalogApi>();
using var response = await factory.SendAsync<CatalogApi>(document, environment, resolver);
```

See [getting started](docs/getting-started.md), [design](docs/design.md), and [release](docs/release.md).
