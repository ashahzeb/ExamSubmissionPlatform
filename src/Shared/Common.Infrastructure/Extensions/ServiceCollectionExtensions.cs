using Common.Application.Abstractions;
using Common.Infrastructure.Caching;
using Common.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Common.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");
        services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(connectionString ?? "localhost:6379"));
        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }

    public static IServiceCollection AddRabbitMessageQueue(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MessageBroker");
        services.AddSingleton<IConnection>(provider =>
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString ?? "amqp://guest:guest@localhost:5672/"),
                DispatchConsumersAsync = true
            };
            return factory.CreateConnection();
        });
        services.AddScoped<IEventPublisher, RabbitMessageQueueEventPublisher>();
        return services;
    }
}