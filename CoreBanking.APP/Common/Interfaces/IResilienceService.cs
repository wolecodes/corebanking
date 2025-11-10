namespace CoreBanking.APP.Common.Interfaces;

public interface IResilienceService
{
    Task<T> ExecuteWithResilienceAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> ExecuteHttpCallWithResilienceAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> httpOperation,
        string operationName,
        CancellationToken cancellationToken = default);
}