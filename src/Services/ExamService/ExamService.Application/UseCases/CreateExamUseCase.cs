using ExamService.Application.Commands;
using ExamService.Application.DTOs;
using ExamService.Application.Abstractions;
using ExamService.Domain.Entities;
using Common.Application.Abstractions;
using Common.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace ExamService.Application.UseCases;

public class CreateExamUseCase(
    IExamRepository examRepository,
    IEventPublisher eventPublisher,
    ITimeZoneService timeZoneService,
    ILogger<CreateExamUseCase> logger)
    : ICommandHandler<CreateExamCommand, CreateExamResult>
{
    private readonly IExamRepository _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
    private readonly IEventPublisher _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    private readonly ITimeZoneService _timeZoneService = timeZoneService ?? throw new ArgumentNullException(nameof(timeZoneService));
    private readonly ILogger<CreateExamUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CreateExamResult> HandleAsync(CreateExamCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating exam: {Title} by user {UserId}", command.Title, command.CreatedByUserId);

        try
        {
            // Validate timezone
            if (!_timeZoneService.IsValidTimeZone(command.TimeZone))
            {
                return CreateExamResult.Failure($"Invalid timezone: {command.TimeZone}");
            }

            // Convert times to UTC for storage
            var startTimeUtc = _timeZoneService.ConvertToUtc(command.StartTime, command.TimeZone);
            var endTimeUtc = _timeZoneService.ConvertToUtc(command.EndTime, command.TimeZone);

            var exam = Exam.Create(
                command.Title,
                command.Description,
                startTimeUtc,
                endTimeUtc,
                command.TimeZone,
                command.DurationMinutes,
                command.CreatedByUserId,
                command.MaxAttempts);

            await _examRepository.AddAsync(exam);

            // Publish event
            var examCreatedEvent = new ExamCreatedEvent(exam.Id, exam.Title, exam.StartTime, exam.EndTime);
            await _eventPublisher.PublishAsync(examCreatedEvent, cancellationToken);

            _logger.LogInformation("Exam created successfully: {ExamId}", exam.Id);

            return CreateExamResult.Success(exam.Id, exam.Title, exam.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exam: {Title}", command.Title);
            return CreateExamResult.Failure("An error occurred while creating the exam");
        }
    }
}