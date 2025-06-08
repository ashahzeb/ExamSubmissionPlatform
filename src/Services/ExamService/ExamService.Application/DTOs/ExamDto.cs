using ExamService.Domain.Entities;

namespace ExamService.Application.DTOs;

public class ExamDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public ExamStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public int MaxAttempts { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}