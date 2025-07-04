using Common.Application.Abstractions;
using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Queries;

public class GetExamSubmissionsQuery : IQuery<IEnumerable<SubmissionDto>>
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}