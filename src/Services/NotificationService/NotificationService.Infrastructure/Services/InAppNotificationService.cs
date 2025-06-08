using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Services;

public class InAppNotificationService(ILogger<InAppNotificationService> logger) : INotificationService
{
    private readonly ILogger<InAppNotificationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> SendAsync(Notification notification)
    {
        if (notification.Type != NotificationType.InApp)
        {
            return false;
        }

        try
        {
            // Implementation would use SignalR or WebSockets for real-time notifications
            _logger.LogInformation("In-app notification sent to user {UserId}: {Message}", 
                notification.UserId, notification.Message);
            
            await Task.Delay(50); // Simulate processing
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send in-app notification to user {UserId}", notification.UserId);
            return false;
        }
    }
}