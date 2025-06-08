using NotificationService.Application.Abstractions;

namespace NotificationService.Api.BackgroundServices;

public class NotificationProcessorService(
    IServiceProvider serviceProvider,
    ILogger<NotificationProcessorService> logger,
    IConfiguration configuration)
    : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<NotificationProcessorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = _configuration.GetValue<int>("NotificationProcessor:IntervalSeconds", 30);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotifications();
                await ProcessFailedNotifications();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notifications");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
        }
    }

    private async Task ProcessPendingNotifications()
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var pendingNotifications = await notificationRepository.GetPendingNotificationsAsync();

        foreach (var notification in pendingNotifications)
        {
            try
            {
                var success = await notificationService.SendAsync(notification);
                
                if (success)
                {
                    notification.MarkAsSent();
                }
                else
                {
                    notification.MarkAsFailed("Failed to send notification");
                }
                
                await notificationRepository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification {NotificationId}", notification.Id);
                notification.MarkAsFailed(ex.Message);
                await notificationRepository.UpdateAsync(notification);
            }
        }
    }

    private async Task ProcessFailedNotifications()
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var failedNotifications = await notificationRepository.GetFailedNotificationsAsync();

        foreach (var notification in failedNotifications.Where(n => n.CanRetry()))
        {
            try
            {
                var success = await notificationService.SendAsync(notification);
                
                if (success)
                {
                    notification.MarkAsSent();
                    await notificationRepository.UpdateAsync(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying notification {NotificationId}", notification.Id);
            }
        }
    }
}