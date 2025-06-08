using Common.Application.Abstractions;
using ExamService.Application.DTOs;

namespace ExamService.Application.Queries;

public class GetExamQuery : IQuery<ExamDto>
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
}