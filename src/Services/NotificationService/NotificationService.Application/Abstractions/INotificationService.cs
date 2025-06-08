using NotificationService.Domain.Entities;

namespace NotificationService.Application.Abstractions;

public interface INotificationService
{
    Task<bool> SendAsync(Notification notification);
}