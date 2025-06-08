using Common.Application.Abstractions;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Common.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Messaging;

public class RabbitMQEventPublisher(IRabbitMQConnection connection, ILogger<RabbitMQEventPublisher> logger)
    : IEventPublisher
{
    private readonly IRabbitMQConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly ILogger<RabbitMQEventPublisher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent
    {
        using var channel = _connection.CreateModel();

        var exchangeName = "exam.platform.events";
        var routingKey = typeof(T).Name.ToLower().Replace("event", "");

        // Declare exchange
        channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

        var eventJson = JsonSerializer.Serialize(@event, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(eventJson);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = @event.Id.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = typeof(T).Name;

        channel.BasicPublish(exchangeName, routingKey, properties, body);

        _logger.LogDebug("Published event {EventType} with ID {EventId}", typeof(T).Name, @event.Id);
    }
}