using Common.Infrastructure.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseCorrelationId();
        
        return app;
    }

    public static WebApplication UseCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }

    public static async Task<WebApplication> MigrateDatabase<TContext>(this WebApplication app) 
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        
        try
        {
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
                app.Logger.LogInformation("Database migration completed for {ContextType}", typeof(TContext).Name);
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database migration failed for {ContextType}", typeof(TContext).Name);
            throw;
        }

        return app;
    }
}