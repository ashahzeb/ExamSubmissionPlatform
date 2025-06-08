using Common.Domain.Abstractions;
using Common.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Common.Infrastructure.Resilience;

public class CircuitBreakerService<T> : ICircuitBreakerService<T>
{
    private readonly IAsyncPolicy<T> _circuitBreakerPolicy;
    private readonly ILogger<CircuitBreakerService<T>> _logger;

    public CircuitBreakerService(ILogger<CircuitBreakerService<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .Or<InvalidOperationException>(ex => ex.Message == "Service returned null result")
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: OnCircuitBreakerOpened,
                onReset: OnCircuitBreakerClosed,
                onHalfOpen: OnCircuitBreakerHalfOpen)
            .AsAsyncPolicy<T>();
    }

    public async Task<T> ExecuteAsync(Func<Task<T>> operation)
    {
        try
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var result = await operation();
                // Convert null results to exceptions so they're counted by the circuit breaker
                if (result == null)
                {
                    _logger.LogWarning("Service returned null result, treating as failure");
                    throw new InvalidOperationException("Service returned null result");
                }
                return result;
            });
        }
        catch (CircuitBreakerOpenException ex)
        {
            _logger.LogWarning("Circuit breaker is open: {Message}", ex.Message);
            throw new ServiceUnavailableException("Service is temporarily unavailable due to circuit breaker", ex);
        }
        catch (InvalidOperationException ex) when (ex.Message == "Service returned null result")
        {
            // Re-throw the original exception without the circuit breaker wrapper
            throw new ServiceUnavailableException("Service returned null result", ex);
        }
    }

    private void OnCircuitBreakerOpened(Exception exception, TimeSpan duration)
    {
        _logger.LogWarning("Circuit breaker opened for {Duration}ms due to exception: {Exception}", 
            duration.TotalMilliseconds, exception.Message);
    }

    private void OnCircuitBreakerClosed()
    {
        _logger.LogInformation("Circuit breaker closed - service recovered");
    }

    private void OnCircuitBreakerHalfOpen()
    {
        _logger.LogInformation("Circuit breaker half-open - testing service");
    }
}