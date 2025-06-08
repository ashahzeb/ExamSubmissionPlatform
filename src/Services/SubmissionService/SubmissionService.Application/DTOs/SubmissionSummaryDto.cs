using SubmissionService.Domain.Entities;

namespace SubmissionService.Application.DTOs;

public class SubmissionSummaryDto
{
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
    public int TotalAttempts { get; set; }
    public DateTime? LastSubmissionAt { get; set; }
    public SubmissionStatus? LastSubmissionStatus { get; set; }
    public bool CanSubmitAgain { get; set; }
}