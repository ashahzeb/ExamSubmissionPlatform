using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Common.Infrastructure.HealthChecks;

public class MessageBrokerHealthCheck : IHealthCheck
{
    private readonly IConnection _connection;
    private readonly ILogger<MessageBrokerHealthCheck> _logger;

    public MessageBrokerHealthCheck(IConnection connection, ILogger<MessageBrokerHealthCheck> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connection.IsOpen)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ connection is closed");
            }

            // Test channel creation
            using var channel = _connection.CreateModel();

            // Test basic operations
            var testQueue = $"health-check-{Guid.NewGuid()}";
            channel.QueueDeclare(testQueue, durable: false, exclusive: true, autoDelete: true);
            channel.QueueDelete(testQueue);

            return HealthCheckResult.Healthy("Message broker is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message broker health check failed");
            return HealthCheckResult.Unhealthy("Message broker is not accessible", ex);
        }
    }
}