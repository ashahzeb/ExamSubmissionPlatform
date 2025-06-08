using Common.Domain.Abstractions;
using Common.Infrastructure.Resilience;
using ExamService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExamService.Infrastructure.Data;

public class ExamDbContext(DbContextOptions<ExamDbContext> options, IRetryService retryService, ILogger<ExamDbContext> logger) : ResilientDbContext(options, retryService, logger)
{
    public DbSet<Exam> Exams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.TimeZone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired();
            
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => new { e.StartTime, e.EndTime });
        });
    }
}