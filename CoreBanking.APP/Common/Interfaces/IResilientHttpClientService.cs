namespace CoreBanking.APP.Common.Interfaces
{
  public interface IResilientHttpClientService
  {
    Task<TResponse> ExecuteWithResilienceAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> action,
        string operationName,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> ExecuteHttpRequestWithResilienceAsync(
        Func<Task<HttpResponseMessage>> request,
        string operationName,
        CancellationToken cancellationToken = default);
  }
}