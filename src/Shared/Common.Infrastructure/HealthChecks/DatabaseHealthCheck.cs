using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.HealthChecks;

public class DatabaseHealthCheck<TContext> : IHealthCheck where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ILogger<DatabaseHealthCheck<TContext>> _logger;

    public DatabaseHealthCheck(TContext context, ILogger<DatabaseHealthCheck<TContext>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple connectivity test
            var stopwatch = Stopwatch.StartNew();
            
            // Use CanConnectAsync
            await _context.Database.CanConnectAsync(cancellationToken);
            stopwatch.Stop();

            var responseTime = stopwatch.ElapsedMilliseconds;

            if (responseTime > 5000) // 5 seconds threshold
            {
                return HealthCheckResult.Degraded($"Database response time is high: {responseTime}ms");
            }

            return HealthCheckResult.Healthy($"Database is healthy. Response time: {responseTime}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is not accessible", ex);
        }
    }
}