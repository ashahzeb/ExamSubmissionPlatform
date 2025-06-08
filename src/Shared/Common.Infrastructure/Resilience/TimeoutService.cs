using Common.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Resilience;

public class TimeoutService(ILogger<TimeoutService> logger) : ITimeoutService
{
    private readonly ILogger<TimeoutService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        string operationName = null)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeout);
        
        try
        {
            return await operation(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException) when (cancellationTokenSource.Token.IsCancellationRequested)
        {
            var message = $"Operation {operationName ?? "unknown"} timed out after {timeout.TotalSeconds} seconds";
            _logger.LogWarning(message);
            throw new TimeoutException(message);
        }
    }
}