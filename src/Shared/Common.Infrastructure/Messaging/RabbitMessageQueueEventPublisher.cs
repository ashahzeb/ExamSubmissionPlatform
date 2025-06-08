using Common.Application.Abstractions;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Common.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Messaging;

public class RabbitMessageQueueEventPublisher(IConnection connection, ILogger<RabbitMessageQueueEventPublisher> logger)
    : IEventPublisher
{
    private readonly IConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly ILogger<RabbitMessageQueueEventPublisher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent
    {
        try
        {
            using var channel = _connection.CreateModel();
            var exchangeName = "exam.platform.events";
            var routingKey = typeof(T).Name.ToLower().Replace("event", "");

            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            var eventJson = JsonSerializer.Serialize(@event, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(eventJson);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = @event.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            channel.BasicPublish(exchangeName, routingKey, properties, body);
            _logger.LogInformation("Published event {EventType} with ID {EventId}", typeof(T).Name, @event.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType}", typeof(T).Name);
            throw;
        }
    }
}