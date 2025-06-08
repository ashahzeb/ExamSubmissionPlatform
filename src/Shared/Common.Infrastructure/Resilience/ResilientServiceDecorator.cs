using Common.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Resilience;

public class ResilientServiceDecorator<TService>(
    TService innerService,
    ICircuitBreakerService<object> circuitBreaker,
    IRetryService retryService,
    ITimeoutService timeoutService,
    ILogger logger)
    where TService : class
{
    protected readonly TService _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
    private readonly ICircuitBreakerService<object> _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
    private readonly IRetryService _retryService = retryService ?? throw new ArgumentNullException(nameof(retryService));
    private readonly ITimeoutService _timeoutService = timeoutService ?? throw new ArgumentNullException(nameof(timeoutService));
    protected readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected async Task<T> ExecuteResilientlyAsync<T>(
        Func<Task<T>> operation, 
        string operationName,
        TimeSpan? timeout = null)
    {
        return await _retryService.ExecuteAsync(async () =>
        {
            var result = await _circuitBreaker.ExecuteAsync(async () =>
            {
                if (timeout.HasValue)
                {
                    var operationResult = await _timeoutService.ExecuteWithTimeoutAsync(
                        async (ct) => await operation(),
                        timeout.Value,
                        operationName);
                    return (object)operationResult!;
                }
                
                var directResult = await operation();
                return (object)directResult!;
            });
            
            return (T)result;
        });
    }
}