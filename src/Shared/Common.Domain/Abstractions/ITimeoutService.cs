namespace Common.Domain.Abstractions;

public interface ITimeoutService
{
    Task<T> ExecuteWithTimeoutAsync<T>(Func<CancellationToken, Task<T>> operation, TimeSpan timeout, string operationName = null);
}