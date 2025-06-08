using Common.Domain.Entities;

namespace Common.Contracts.Events;

public class ExamSubmittedEvent(Guid submissionId, Guid examId, Guid userId, DateTime submittedAt)
    : BaseEvent
{
    public Guid SubmissionId { get; } = submissionId;
    public Guid ExamId { get; } = examId;
    public Guid UserId { get; } = userId;
    public DateTime SubmittedAt { get; } = submittedAt;
}