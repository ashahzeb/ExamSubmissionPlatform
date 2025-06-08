using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Services;

public class SmsNotificationService(ILogger<SmsNotificationService> logger) : INotificationService
{
    private readonly ILogger<SmsNotificationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> SendAsync(Notification notification)
    {
        if (notification.Type != NotificationType.Sms)
        {
            return false;
        }

        try
        {
            // Implementation would integrate with SMS provider like Twilio
            _logger.LogInformation("SMS would be sent to {Recipient}: {Message}", 
                notification.Recipient, notification.Message);
            
            await Task.Delay(100); // Simulate API call
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Recipient}", notification.Recipient);
            return false;
        }
    }
}