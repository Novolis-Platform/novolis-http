namespace Novolis.Http.Unit.Infrastructure;

/// <summary>
/// Records outbound requests and returns a configurable response (defaults to 200 OK).
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    public List<HttpRequestMessage> SentRequests { get; } = [];

    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? SendAsyncImpl { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SentRequests.Add(request);
        if (SendAsyncImpl is not null)
            return SendAsyncImpl(request, cancellationToken);

        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        });
    }
}
