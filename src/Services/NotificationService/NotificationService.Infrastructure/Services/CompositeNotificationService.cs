using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Services;

public class CompositeNotificationService(
    IEnumerable<INotificationService> services,
    ILogger<CompositeNotificationService> logger)
    : INotificationService
{
    private readonly IEnumerable<INotificationService> _services = services ?? throw new ArgumentNullException(nameof(services));
    private readonly ILogger<CompositeNotificationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> SendAsync(Notification notification)
    {
        foreach (var service in _services)
        {
            try
            {
                var result = await service.SendAsync(notification);
                if (result)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification service {ServiceType}", service.GetType().Name);
            }
        }

        return false;
    }
}