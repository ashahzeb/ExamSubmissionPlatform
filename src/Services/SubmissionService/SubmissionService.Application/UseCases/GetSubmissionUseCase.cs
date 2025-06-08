using SubmissionService.Application.DTOs;
using SubmissionService.Application.Abstractions;
using SubmissionService.Application.Queries;
using Common.Application.Abstractions;
using Common.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace SubmissionService.Application.UseCases;

public class GetSubmissionUseCase(ISubmissionRepository submissionRepository, ILogger<GetSubmissionUseCase> logger)
    : IQueryHandler<GetSubmissionQuery, SubmissionDto>
{
    private readonly ISubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly ILogger<GetSubmissionUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<SubmissionDto> HandleAsync(GetSubmissionQuery query, CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(query.SubmissionId);
        if (submission == null)
        {
            throw new NotFoundException($"Submission not found: {query.SubmissionId}");
        }

        return new SubmissionDto
        {
            Id = submission.Id,
            ExamId = submission.ExamId,
            UserId = submission.UserId,
            Content = submission.Content,
            SubmittedAt = submission.SubmittedAt,
            Status = submission.Status,
            AttemptNumber = submission.AttemptNumber,
            TimeTaken = submission.TimeTaken,
            FailureReason = submission.FailureReason,
            CreatedAt = submission.CreatedAt
        };
    }
}