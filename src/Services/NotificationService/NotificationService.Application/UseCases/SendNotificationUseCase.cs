using Common.Application.Abstractions;
using NotificationService.Application.Commands;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Results;

namespace NotificationService.Application.UseCases;

public class SendNotificationUseCase(
    INotificationRepository notificationRepository,
    INotificationService notificationService,
    ILogger<SendNotificationUseCase> logger)
    : ICommandHandler<SendNotificationCommand, SendNotificationResult>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private readonly ILogger<SendNotificationUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<SendNotificationResult> HandleAsync(SendNotificationCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending {Type} notification to {Recipient}", command.Type, command.Recipient);

        try
        {
            // Create notification entity
            var notification = Notification.Create(
                command.UserId,
                command.Type,
                command.Title,
                command.Message,
                command.Recipient);

            // Add metadata
            foreach (var item in command.Data)
            {
                notification.AddMetadata(item.Key, item.Value);
            }

            // Save to database first
            await _notificationRepository.AddAsync(notification);

            // Send notification
            var success = await _notificationService.SendAsync(notification);

            if (success)
            {
                notification.MarkAsSent();
                _logger.LogInformation("Notification sent successfully: {NotificationId}", notification.Id);
            }
            else
            {
                notification.MarkAsFailed("Failed to send notification");
                _logger.LogWarning("Failed to send notification: {NotificationId}", notification.Id);
            }

            await _notificationRepository.UpdateAsync(notification);

            return success 
                ? SendNotificationResult.Success(notification.Id)
                : SendNotificationResult.Failure("Failed to send notification");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return SendNotificationResult.Failure("An error occurred while sending the notification");
        }
    }
}