using Common.Application.Abstractions;
using Common.Domain;
using Common.Domain.Abstractions;
using Common.Domain.Entities;
using Common.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Data;

public abstract class BaseDbContext(
    DbContextOptions options,
    IRetryService retryService,
    ILogger<ResilientDbContext> logger)
    : ResilientDbContext(options, retryService, logger), IDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.Id))
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.CreatedAt))
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.UpdatedAt));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.CreatedAt));
            }
        }
    }
    
    //This is just for the sake of test project, in actual development new migrations will be created
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => 
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}