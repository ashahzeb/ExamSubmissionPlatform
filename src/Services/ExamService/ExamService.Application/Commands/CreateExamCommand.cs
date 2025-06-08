using Common.Domain;
using Common.Domain.Entities;

namespace ExamService.Application.Commands;

public class CreateExamCommand : BaseCommand
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public int MaxAttempts { get; set; } = 1;
}