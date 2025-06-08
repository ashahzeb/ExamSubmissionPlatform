using NotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Common.Domain.Abstractions;
using Common.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options, IRetryService retryService, ILogger<NotificationDbContext> logger) : ResilientDbContext(options, retryService, logger)
{
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
            
            // Store metadata as JSON
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}