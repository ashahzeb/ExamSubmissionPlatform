using Common.Domain;
using Common.Domain.Entities;

namespace SubmissionService.Application.Commands;

public class SubmitExamCommand : BaseCommand
{
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}