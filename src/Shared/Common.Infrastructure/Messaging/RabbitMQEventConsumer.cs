using System.Text;
using System.Text.Json;
using Common.Application.Abstractions;
using Common.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common.Infrastructure.Messaging;

public class RabbitMQEventConsumer<T>(
    IRabbitMQConnection connection,
    IServiceProvider serviceProvider,
    ILogger<RabbitMQEventConsumer<T>> logger)
    : BackgroundService
    where T : class, IEvent
{
    private readonly IRabbitMQConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<RabbitMQEventConsumer<T>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _channel = _connection.CreateModel();
        
        var exchangeName = "exam.platform.events";
        var queueName = $"{typeof(T).Name.ToLower().Replace("event", "")}.queue";
        var routingKey = typeof(T).Name.ToLower().Replace("event", "");

        _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, exchangeName, routingKey);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnEventReceived;

        _channel.BasicConsume(queueName, autoAck: false, consumer);
        _logger.LogInformation("Started consuming events of type {EventType}", typeof(T).Name);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnEventReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = typeof(T).Name;
        var messageId = eventArgs.BasicProperties.MessageId;

        try
        {
            var body = eventArgs.Body.ToArray();
            var eventJson = Encoding.UTF8.GetString(body);
            var @event = JsonSerializer.Deserialize<T>(eventJson, _jsonOptions);

            if (@event == null)
            {
                _logger.LogWarning("Failed to deserialize event {EventType} with message ID {MessageId}", eventName, messageId);
                _channel?.BasicNack(eventArgs.DeliveryTag, false, false);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event);
            }

            _channel?.BasicAck(eventArgs.DeliveryTag, false);
            _logger.LogDebug("Successfully processed event {EventType} with ID {EventId}", eventName, @event.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventType} with message ID {MessageId}", eventName, messageId);
            
            // Retry logic - for now, just reject and requeue
            _channel?.BasicNack(eventArgs.DeliveryTag, false, true);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}