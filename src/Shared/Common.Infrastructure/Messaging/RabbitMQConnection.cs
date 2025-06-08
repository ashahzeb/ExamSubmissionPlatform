using System.Net.Sockets;
using Common.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Common.Infrastructure.Messaging;

public class RabbitMQConnection : IRabbitMQConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMQConnection> _logger;
    private IConnection? _connection;
    private bool _disposed;
    private readonly object _lock = new();

    public RabbitMQConnection(IConfiguration configuration, ILogger<RabbitMQConnection> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var connectionString = configuration.GetConnectionString("MessageBroker") 
            ?? throw new InvalidOperationException("MessageBroker connection string not found");

        _connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            TopologyRecoveryEnabled = true,
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            DispatchConsumersAsync = true
        };
    }

    public bool IsConnected => _connection?.IsOpen == true;

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            Reconnect();
        }

        return _connection?.CreateModel() ?? throw new InvalidOperationException("RabbitMQ connection is not established");
    }

    public void Reconnect()
    {
        lock (_lock)
        {
            if (_disposed) return;

            try
            {
                _connection?.Dispose();
                _connection = _connectionFactory.CreateConnection();
                
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _connection.ConnectionUnblocked += OnConnectionUnblocked;

                _logger.LogInformation("RabbitMQ connection established");
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ broker");
                throw;
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Socket error while connecting to RabbitMQ");
                throw;
            }
        }
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        if (_disposed) return;
        
        _logger.LogWarning("RabbitMQ connection shutdown: {Reason}", e.ReplyText);
    }

    private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
    {
        if (_disposed) return;
        
        _logger.LogError(e.Exception, "RabbitMQ callback exception");
    }

    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogWarning("RabbitMQ connection blocked: {Reason}", e.Reason);
    }

    private void OnConnectionUnblocked(object? sender, EventArgs e)
    {
        _logger.LogInformation("RabbitMQ connection unblocked");
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}