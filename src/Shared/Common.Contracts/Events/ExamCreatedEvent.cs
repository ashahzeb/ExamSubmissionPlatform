using Common.Domain.Entities;

namespace Common.Contracts.Events;

public class ExamCreatedEvent(Guid examId, string title, DateTime startTime, DateTime endTime)
    : BaseEvent
{
    public Guid ExamId { get; } = examId;
    public string Title { get; } = title;
    public DateTime StartTime { get; } = startTime;
    public DateTime EndTime { get; } = endTime;
}