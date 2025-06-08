using Common.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using Polly;

namespace Common.Infrastructure.Resilience;

public class RetryService(ILogger<RetryService> logger) : IRetryService
{
    private readonly ILogger<RetryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry attempt {RetryCount} after {Delay}ms: {Exception}", 
                        retryCount, timespan.TotalMilliseconds, outcome?.Message);
                });

        return await retryPolicy.ExecuteAsync(operation);
    }

    public async Task ExecuteAsync(Func<Task> operation, int maxRetries = 3)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry attempt {RetryCount} after {Delay}ms: {Exception}", 
                        retryCount, timespan.TotalMilliseconds, outcome?.Message);
                });

        await retryPolicy.ExecuteAsync(operation);
    }
}