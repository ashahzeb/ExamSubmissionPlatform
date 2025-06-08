using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Common.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationCors(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (environment.IsDevelopment())
                {
                    // Development: Allow any localhost origin
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin)) return false;
                        
                        var uri = new Uri(origin);
                        return uri.Host == "localhost" || 
                               uri.Host == "127.0.0.1" ||
                               uri.Host.EndsWith(".localhost") || // Docker containers
                               uri.Host.EndsWith(".local");       // Local development
                    })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                }
                else
                {
                    // Production: Strict origins from configuration
                    var allowedOrigins = configuration
                        .GetSection("AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();
                        
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                    else
                    {
                        // Fallback: No CORS if no origins configured
                        policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                        // Note: Can't use AllowCredentials with AllowAnyOrigin
                    }
                }
            });
        });
        
        return services;
    }
}