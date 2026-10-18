# Novolis.Http.Authentication

`DelegatingHandler` authentication for named Novolis HTTP clients.

```powershell
dotnet add package Novolis.Http.Authentication
```

Chain a helper on `AddHttpClientFor<TApi>()`:

```csharp
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

Helpers:

- `AddBasicAuthentication`
- `AddApiKeyAuthentication`
- `AddBearerAuthentication`
- `AddClientCredentialsAuthentication`
- `AddRefreshTokenAuthentication`

Token grants cache access tokens in memory, honor `expires_in`, refresh when a refresh token is stored, and retry a `401` once. A `401` expires the access token and keeps any refresh token. Token endpoint calls use the `novolis-token` client, which has no authentication handler. Call `IHttpTokenCache.Invalidate<TApi>()` when a secret rotates.
