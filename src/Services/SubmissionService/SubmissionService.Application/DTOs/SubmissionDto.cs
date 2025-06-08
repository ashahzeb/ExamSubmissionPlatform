using SubmissionService.Domain.Entities;

namespace SubmissionService.Application.DTOs;

public class SubmissionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public TimeSpan TimeTaken { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}