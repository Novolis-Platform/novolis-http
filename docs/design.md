# Design

`novolis-http` separates the saved request model from request execution:

1. `Novolis.Http.Documents` stores requests and scoped environments.
2. `Novolis.Http.Variables` resolves environment values asynchronously.
3. `Novolis.Http.Client` names an `HttpClient` with an `HttpClientKey` and sends through `IHttpClientFactory`.
4. `Novolis.Http.Authentication` applies credentials in `DelegatingHandler`s on that named client.

The document model contains no presentation types. Hosts can persist the records as JSON, display them in a UI, replay them from a utility, or use them in tests.

A product type is only a sealed key. `GetClientName()` is implemented on `HttpClientKey` and returns the derived type's full name:

```csharp
public sealed class GitApi() : HttpClientKey;
```

`GitHub.Client.GitApi` and `GitLab.Client.GitApi` are different factory names. The short name `GitApi` is not used. `AssemblyQualifiedName` is not used, so a package version bump does not rename the client. Registration, options, the token cache, and persisted `ClientName` all use that normalized full name. Callers pass `GitApi`, not a string. A JSON document stores `ClientName = new GitApi().GetClientName()` when it must name the client without a CLR type argument.

Token grants share an `IMemoryCache` store and a separate `novolis-token` client that has no authentication handler. Lifetime comes from `expires_in` minus a 60-second skew. Shared acquire does not take the first caller's cancellation token. A `401` expires the access token, keeps any refresh token, and retries the resource request once. `Invalidate` / `Invalidate<TApi>()` remain a full wipe for secret rotation.

The old `Novolis.Transports.Http.*` package family is retired. Transport packages remain focused on TCP, IPC, torrent, and packet movement.
