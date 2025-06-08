using Common.Application.Abstractions;
using Common.Contracts.Events;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Commands;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Results;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.EventHandlers;

public class ExamSubmittedEventHandler : IEventHandler<ExamSubmittedEvent>
{
    private readonly ICommandHandler<SendNotificationCommand, SendNotificationResult> _sendNotificationHandler;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IExamServiceClient _examServiceClient;
    private readonly ILogger<ExamSubmittedEventHandler> _logger;

    public ExamSubmittedEventHandler(
        ICommandHandler<SendNotificationCommand, SendNotificationResult> sendNotificationHandler,
        IUserServiceClient userServiceClient,
        IExamServiceClient examServiceClient,
        ILogger<ExamSubmittedEventHandler> logger)
    {
        _sendNotificationHandler = sendNotificationHandler ?? throw new ArgumentNullException(nameof(sendNotificationHandler));
        _userServiceClient = userServiceClient ?? throw new ArgumentNullException(nameof(userServiceClient));
        _examServiceClient = examServiceClient ?? throw new ArgumentNullException(nameof(examServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(ExamSubmittedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing ExamSubmittedEvent for submission {SubmissionId}", @event.SubmissionId);

            // Get user and exam details
            var user = await _userServiceClient.GetUserAsync(@event.UserId);
            var exam = await _examServiceClient.GetExamAsync(@event.ExamId);

            if (user == null || exam == null)
            {
                _logger.LogWarning("User or exam not found for submission {SubmissionId}", @event.SubmissionId);
                return;
            }

            // Send email notification
            var emailCommand = new SendNotificationCommand
            {
                UserId = @event.UserId,
                Type = NotificationType.Email,
                Title = "Exam Submission Confirmation",
                Message = $"Your submission for exam '{exam.Title}' has been received successfully.",
                Recipient = user.Email,
                Data = new Dictionary<string, object>
                {
                    ["examTitle"] = exam.Title,
                    ["submissionId"] = @event.SubmissionId,
                    ["submittedAt"] = @event.SubmittedAt,
                    ["userName"] = user.FullName
                }
            };

            await _sendNotificationHandler.HandleAsync(emailCommand, cancellationToken);

            // Send in-app notification
            var inAppCommand = new SendNotificationCommand
            {
                UserId = @event.UserId,
                Type = NotificationType.InApp,
                Title = "Submission Received",
                Message = $"Your submission for '{exam.Title}' has been received.",
                Recipient = @event.UserId.ToString(),
                Data = emailCommand.Data
            };

            await _sendNotificationHandler.HandleAsync(inAppCommand, cancellationToken);

            _logger.LogInformation("Notifications sent for submission {SubmissionId}", @event.SubmissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ExamSubmittedEvent for submission {SubmissionId}", @event.SubmissionId);
        }
    }
}