using Common.Application.Abstractions;
using ExamService.Application.DTOs;

namespace ExamService.Application.Queries;

public class GetActiveExamsQuery : IQuery<IEnumerable<ExamDto>>
{
    public Guid Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}