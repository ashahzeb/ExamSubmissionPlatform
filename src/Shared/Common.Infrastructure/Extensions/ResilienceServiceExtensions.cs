using Common.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Common.Infrastructure.Resilience;
using Common.Infrastructure.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Extensions;

public static class ResilienceServiceExtensions
{
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
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck<TDbContext>>("database")
            .AddCheck<MessageBrokerHealthCheck>("messagebroker")
            .AddCheck<RedisHealthCheck>("redis");
        
        return services;
    }
}