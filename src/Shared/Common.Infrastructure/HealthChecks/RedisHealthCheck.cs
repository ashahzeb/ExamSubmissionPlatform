using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Infrastructure.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IServiceProvider serviceProvider, ILogger<RedisHealthCheck> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Try to get Redis connection, but don't fail if it doesn't exist
        try
        {
            _redis = serviceProvider.GetService<IConnectionMultiplexer>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve Redis connection for health check");
            _redis = null;
        }
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // If Redis is not configured, report as healthy (optional dependency)
            if (_redis == null)
            {
                _logger.LogDebug("Redis is not configured, skipping health check");
                return HealthCheckResult.Healthy("Redis is not configured (optional)");
            }

            // Check if Redis is connected
            if (!_redis.IsConnected)
            {
                return HealthCheckResult.Degraded("Redis is not connected");
            }

            var database = _redis.GetDatabase();
            var testKey = $"health-check-{Guid.NewGuid()}";
            
            // Set a test value with timeout
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            
            await database.StringSetAsync(testKey, "test-value", TimeSpan.FromSeconds(10));
            var value = await database.StringGetAsync(testKey);
            await database.KeyDeleteAsync(testKey);

            if (value == "test-value")
            {
                return HealthCheckResult.Healthy("Redis is healthy");
            }

            return HealthCheckResult.Degraded("Redis responded but with unexpected value");
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection failed during health check");
            return HealthCheckResult.Degraded("Redis connection failed", ex);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Redis health check timed out");
            return HealthCheckResult.Degraded("Redis health check timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed unexpectedly");
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}