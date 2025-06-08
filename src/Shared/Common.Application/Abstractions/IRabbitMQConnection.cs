using RabbitMQ.Client;

namespace Common.Application.Abstractions;

public interface IRabbitMQConnection : IDisposable
{
    bool IsConnected { get; }
    IModel CreateModel();
    void Reconnect();
}