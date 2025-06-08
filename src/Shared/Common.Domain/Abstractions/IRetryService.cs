namespace Common.Domain.Abstractions;

public interface IRetryService
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, int maxRetries = 3);
    Task ExecuteAsync(Func<Task> operation, int maxRetries = 3);
}