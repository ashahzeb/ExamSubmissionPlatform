using Common.Domain.Entities;

namespace ExamService.Application.Commands;

public class PublishExamCommand : BaseCommand
{
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
}