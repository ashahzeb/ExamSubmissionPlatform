using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository(NotificationDbContext context, ILogger<NotificationRepository> logger)
    : INotificationRepository
{
    private readonly NotificationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<NotificationRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount = 100)
    {
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetFailedNotificationsAsync(int maxCount = 100)
    {
        return await _context.Notifications
            .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < 3)
            .OrderBy(n => n.CreatedAt)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Notification created: {NotificationId}", notification.Id);
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Notification updated: {NotificationId}", notification.Id);
    }
}