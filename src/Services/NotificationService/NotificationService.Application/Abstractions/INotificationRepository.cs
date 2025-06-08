using NotificationService.Domain.Entities;

namespace NotificationService.Application.Abstractions;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount = 100);
    Task<IEnumerable<Notification>> GetFailedNotificationsAsync(int maxCount = 100);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
}