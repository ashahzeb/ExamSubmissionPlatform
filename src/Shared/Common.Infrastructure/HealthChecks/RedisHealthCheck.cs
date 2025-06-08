using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Infrastructure.HealthChecks;

public class RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    private readonly ILogger<RedisHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            var testKey = $"health-check-{Guid.NewGuid()}";
            
            await database.StringSetAsync(testKey, "test-value", TimeSpan.FromSeconds(10));
            var value = await database.StringGetAsync(testKey);
            await database.KeyDeleteAsync(testKey);

            if (value == "test-value")
            {
                return HealthCheckResult.Healthy("Redis is healthy");
            }

            return HealthCheckResult.Degraded("Redis responded but with unexpected value");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis is not accessible", ex);
        }
    }
}