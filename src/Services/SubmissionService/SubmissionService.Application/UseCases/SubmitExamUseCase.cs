using Common.Application.Abstractions;
using SubmissionService.Application.Commands;
using SubmissionService.Domain.Entities;
using Common.Contracts.Events;
using Microsoft.Extensions.Logging;
using SubmissionService.Application.Abstractions;
using SubmissionService.Application.Results;

namespace SubmissionService.Application.UseCases;

public class SubmitExamUseCase(
    ISubmissionRepository submissionRepository,
    IExamServiceClient examServiceClient,
    IEventPublisher eventPublisher,
    ILogger<SubmitExamUseCase> logger)
    : ICommandHandler<SubmitExamCommand, SubmitExamResult>
{
    private readonly ISubmissionRepository _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
    private readonly IExamServiceClient _examServiceClient = examServiceClient ?? throw new ArgumentNullException(nameof(examServiceClient));
    private readonly IEventPublisher _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    private readonly ILogger<SubmitExamUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<SubmitExamResult> HandleAsync(SubmitExamCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing exam submission for User {UserId}, Exam {ExamId}", 
            command.UserId, command.ExamId);

        try
        {
            // 1. Validate exam exists and is active
            var examDetails = await _examServiceClient.GetExamAsync(command.ExamId);
            if (examDetails == null)
            {
                _logger.LogWarning("Exam not found: {ExamId}", command.ExamId);
                return SubmitExamResult.Failure("Exam not found");
            }

            if (!examDetails.IsActive)
            {
                _logger.LogWarning("Exam is not active: {ExamId}", command.ExamId);
                return SubmitExamResult.Failure("Exam is not active");
            }

            // 2. Check submission window
            var currentTime = DateTime.UtcNow;
            if (currentTime < examDetails.StartTime || currentTime > examDetails.EndTime)
            {
                _logger.LogWarning("Submission outside time window for exam {ExamId}", command.ExamId);
                return SubmitExamResult.Failure("Submission deadline has passed");
            }

            // 3. Check attempt limits
            var existingSubmissions = await _submissionRepository.GetByUserAndExamAsync(command.UserId, command.ExamId);
            var attemptNumber = existingSubmissions.Count() + 1;

            if (attemptNumber > examDetails.MaxAttempts)
            {
                _logger.LogWarning("Max attempts exceeded for User {UserId}, Exam {ExamId}", 
                    command.UserId, command.ExamId);
                return SubmitExamResult.Failure($"Maximum attempts ({examDetails.MaxAttempts}) exceeded");
            }

            // 4. Calculate time taken
            var timeTaken = DateTime.UtcNow - command.StartedAt;
            if (timeTaken.TotalMinutes > examDetails.DurationMinutes)
            {
                _logger.LogWarning("Submission time exceeded for User {UserId}, Exam {ExamId}", 
                    command.UserId, command.ExamId);
                return SubmitExamResult.Failure("Exam duration exceeded");
            }

            // 5. Create and save submission
            var submission = ExamSubmission.Create(
                command.ExamId,
                command.UserId,
                command.Content,
                DateTime.UtcNow,
                attemptNumber,
                timeTaken,
                command.UserAgent,
                command.IpAddress);

            await _submissionRepository.AddAsync(submission);

            // 6. Publish event
            var submittedEvent = new ExamSubmittedEvent(
                submission.Id,
                submission.ExamId,
                submission.UserId,
                submission.SubmittedAt);

            await _eventPublisher.PublishAsync(submittedEvent, cancellationToken);

            _logger.LogInformation("Exam submission successful: {SubmissionId}, Attempt: {AttemptNumber}", 
                submission.Id, attemptNumber);

            return SubmitExamResult.Success(submission.Id, submission.SubmittedAt, attemptNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing exam submission for User {UserId}, Exam {ExamId}", 
                command.UserId, command.ExamId);
            return SubmitExamResult.Failure("An error occurred while processing the submission");
        }
    }
}