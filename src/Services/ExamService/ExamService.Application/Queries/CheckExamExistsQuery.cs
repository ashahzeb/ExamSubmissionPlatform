using Common.Application.Abstractions;

namespace ExamService.Application.Queries;

public class CheckExamExistsQuery : IQuery<bool>
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
}