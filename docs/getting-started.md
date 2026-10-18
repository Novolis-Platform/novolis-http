# Getting started

Add the package that matches the host:

```powershell
dotnet add package Novolis.Http.Client
dotnet add package Novolis.Http.Authentication
dotnet add package Novolis.Http.Documents
dotnet add package Novolis.Http.Variables
```

Declare a client key and register it on `IServiceCollection`:

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
```

Create a request document, resolve its environment, and send through the key:

```csharp
var document = new HttpRequestDocument
{
    Method = "GET",
    Uri = "https://catalog.example/items",
};

using var response = await factory.SendAsync<CatalogApi>(
    document,
    environment,
    resolver,
    cancellationToken);
```

A persisted document without a type argument selects the same client when `ClientName` equals `new CatalogApi().GetClientName()`.

The core packages are host-neutral and can be used by Avalonia, Blazor, command-line, tests, and automation.
