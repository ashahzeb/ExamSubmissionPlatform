using System.Diagnostics.Metrics;

namespace Common.Infrastructure.Monitoring;

public class ResilienceMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _circuitBreakerOpenCounter;
    private readonly Counter<long> _retryAttemptCounter;
    private readonly Counter<long> _timeoutCounter;
    private readonly Histogram<double> _operationDurationHistogram;

    public ResilienceMetrics()
    {
        _meter = new Meter("ExamPlatform.Resilience");
        
        _circuitBreakerOpenCounter = _meter.CreateCounter<long>(
            "circuit_breaker_open_total",
            description: "Total number of circuit breaker openings");
            
        _retryAttemptCounter = _meter.CreateCounter<long>(
            "retry_attempts_total",
            description: "Total number of retry attempts");
            
        _timeoutCounter = _meter.CreateCounter<long>(
            "timeouts_total",
            description: "Total number of operation timeouts");
            
        _operationDurationHistogram = _meter.CreateHistogram<double>(
            "operation_duration_seconds",
            description: "Duration of operations in seconds");
    }

    public void RecordCircuitBreakerOpen(string serviceName)
    {
        _circuitBreakerOpenCounter.Add(1, new KeyValuePair<string, object?>("service", serviceName));
    }

    public void RecordRetryAttempt(string operationName, int attemptNumber)
    {
        _retryAttemptCounter.Add(1, 
            new KeyValuePair<string, object?>("operation", operationName),
            new KeyValuePair<string, object?>("attempt", attemptNumber));
    }

    public void RecordTimeout(string operationName)
    {
        _timeoutCounter.Add(1, new KeyValuePair<string, object?>("operation", operationName));
    }

    public void RecordOperationDuration(string operationName, TimeSpan duration, bool successful)
    {
        _operationDurationHistogram.Record(duration.TotalSeconds,
            new KeyValuePair<string, object?>("operation", operationName),
            new KeyValuePair<string, object?>("successful", successful));
    }
}