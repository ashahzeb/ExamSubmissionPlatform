namespace Common.Domain.Abstractions;

public interface ICircuitBreakerService<T>
{
    Task<T> ExecuteAsync(Func<Task<T>> operation);
}