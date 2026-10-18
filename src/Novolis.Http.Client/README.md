# Novolis.Http.Client

Named `HttpClient` registration and document send helpers on `IHttpClientFactory`.

```powershell
dotnet add package Novolis.Http.Client
```

Declare a sealed key, register it, and resolve the same `HttpClient`:

```csharp
public sealed class CatalogApi() : HttpClientKey;

services.AddNovolisHttp();
services.AddHttpClientFor<CatalogApi>(client =>
{
    client.BaseAddress = new Uri("https://catalog.example/");
});

HttpClient catalog = factory.CreateClient<CatalogApi>();
using var response = await factory.SendAsync<CatalogApi>(document, environment, resolver);
```

`CatalogApi` is not constructed by dependency injection and does not take `HttpClient`. `GetClientName()` returns the derived type's full name. A persisted document stores that value in `ClientName`.
