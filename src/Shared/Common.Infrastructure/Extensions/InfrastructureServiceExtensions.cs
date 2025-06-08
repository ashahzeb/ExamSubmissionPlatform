using Common.Application.Abstractions;
using Common.Domain.Abstractions;
using Common.Infrastructure.Caching;
using Common.Infrastructure.Data;
using Common.Infrastructure.HealthChecks;
using Common.Infrastructure.Messaging;
using Common.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Common.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMQ(configuration);
        services.AddRedisCache(configuration);
        services.AddResilience(configuration);
        return services;
    }

    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MessageBroker");
        
        // Register RabbitMQ connection
        services.AddSingleton<IConnection>(provider =>
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString ?? "amqp://guest:guest@localhost:5672/"),
                DispatchConsumersAsync = true
            };
            return factory.CreateConnection();
        });
        
        // Register custom connection wrapper if you have one
        services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
        
        // Register event publisher
        services.AddScoped<IEventPublisher, RabbitMQEventPublisher>();
        
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");
        
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var configOptions = ConfigurationOptions.Parse(connectionString);
                configOptions.AbortOnConnectFail = false;
                configOptions.ConnectRetry = 3;
                configOptions.ConnectTimeout = 30000;
                return ConnectionMultiplexer.Connect(configOptions);
            });
        }
        else
        {
            // Fallback to localhost if no connection string provided
            services.AddSingleton<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect("localhost:6379"));
        }
        
        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }

    public static IServiceCollection AddCustomDbContext<TContext>(
        this IServiceCollection services, 
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection") 
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        
        return services;
    }

    public static IServiceCollection AddEventConsumer<TEvent>(this IServiceCollection services)
        where TEvent : class, IEvent
    {
        services.AddHostedService<RabbitMQEventConsumer<TEvent>>();
        return services;
    }

    public static IServiceCollection AddResilience(this IServiceCollection services, IConfiguration configuration)
    {
        // Circuit Breaker Services
        services.AddSingleton(typeof(ICircuitBreakerService<>), typeof(CircuitBreakerService<>));
        
        // Retry Services
        services.AddScoped<IRetryService, RetryService>();
        
        // Timeout Services
        services.AddScoped<ITimeoutService, TimeoutService>();
        
        // Distributed Locking
        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
        
        // Replace default event publisher with resilient version
        services.AddScoped<ResilientRabbitMessageQueueEventPublisher>();
        
        return services;
    }

    public static IServiceCollection AddCustomHealthChecks<TDbContext>(
        this IServiceCollection services, 
        IConfiguration configuration) where TDbContext : DbContext
    {
        var healthChecksBuilder = services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck<TDbContext>>("database")
            .AddCheck<MessageBrokerHealthCheck>("messagebroker");
        
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddCheck<RedisHealthCheck>("redis");
        }
    
        return services;
    }
}