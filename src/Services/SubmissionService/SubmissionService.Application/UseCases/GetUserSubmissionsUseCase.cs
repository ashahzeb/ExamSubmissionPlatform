using Common.Application.Abstractions;
using SubmissionService.Application.Abstractions;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Queries;

namespace SubmissionService.Application.UseCases;

public class GetUserSubmissionsUseCase(ISubmissionRepository submissionRepository)
    : IQueryHandler<GetUserSubmissionsQuery, IEnumerable<SubmissionDto>>
{
    private readonly ISubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));

    public async Task<IEnumerable<SubmissionDto>> HandleAsync(GetUserSubmissionsQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<SubmissionService.Domain.Entities.ExamSubmission> submissions;

        if (query.ExamId.HasValue)
        {
            submissions = await _submissionRepository.GetByUserAndExamAsync(query.UserId, query.ExamId.Value);
        }
        else
        {
            submissions = await _submissionRepository.GetByUserAsync(query.UserId, query.PageNumber, query.PageSize);
        }

        return submissions.Select(s => new SubmissionDto
        {
            Id = s.Id,
            ExamId = s.ExamId,
            UserId = s.UserId,
            Content = s.Content,
            SubmittedAt = s.SubmittedAt,
            Status = s.Status,
            AttemptNumber = s.AttemptNumber,
            TimeTaken = s.TimeTaken,
            FailureReason = s.FailureReason,
            CreatedAt = s.CreatedAt
        });
    }
}