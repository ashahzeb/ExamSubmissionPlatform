using Common.Domain.Abstractions;
using Common.Infrastructure.Resilience;
using SubmissionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SubmissionService.Infrastructure.Data;

public class SubmissionDbContext(DbContextOptions<SubmissionDbContext> options, IRetryService retryService, ILogger<SubmissionDbContext> logger) : ResilientDbContext(options, retryService, logger)
{
    public DbSet<ExamSubmission> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExamSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExamId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.AttemptNumber).IsRequired();
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            
            // Indexes for performance
            entity.HasIndex(e => new { e.UserId, e.ExamId });
            entity.HasIndex(e => e.ExamId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SubmittedAt);
            entity.HasIndex(e => e.Status);
        });
    }
}