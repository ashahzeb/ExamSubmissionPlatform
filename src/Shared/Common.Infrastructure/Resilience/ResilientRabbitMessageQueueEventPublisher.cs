using Common.Application.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using Common.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Resilience;

public class ResilientRabbitMessageQueueEventPublisher(
    IConnection connection,
    IRetryService retryService,
    ICircuitBreakerService<object> circuitBreaker,
    ILogger<ResilientRabbitMessageQueueEventPublisher> logger)
    : IEventPublisher
{
    private readonly IConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly IRetryService _retryService = retryService ?? throw new ArgumentNullException(nameof(retryService));
    private readonly ICircuitBreakerService<object> _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
    private readonly ILogger<ResilientRabbitMessageQueueEventPublisher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            await _retryService.ExecuteAsync(async () =>
            {
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    await PublishEventInternal(@event);
                    return Task.CompletedTask;
                });
            });
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    private async Task PublishEventInternal<T>(T @event) where T : IEvent
    {
        try
        {
            using var channel = _connection.CreateModel();

            var exchangeName = "exam.platform.events";
            var routingKey = typeof(T).Name.ToLower().Replace("event", "");

            // Declare exchange with durability
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            var eventJson = JsonSerializer.Serialize(@event, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(eventJson);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; // Message durability
            properties.MessageId = @event.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            // Confirm mode for reliability
            channel.ConfirmSelect();

            channel.BasicPublish(exchangeName, routingKey, properties, body);

            // Wait for confirmation
            if (!channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
            {
                throw new InvalidOperationException("Message publish was not confirmed by broker");
            }

            _logger.LogDebug("Published event {EventType} with ID {EventId}", typeof(T).Name, @event.Id);
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogError(ex, "RabbitMQ broker unreachable");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType}", typeof(T).Name);
            throw;
        }
    }
}