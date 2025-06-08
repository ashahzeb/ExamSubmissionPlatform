using Common.Application.Abstractions;
using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Queries;

public class GetSubmissionQuery : IQuery<SubmissionDto>
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
}