using Common.Domain;
using Common.Domain.Entities;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Commands;

public class SendNotificationCommand : BaseCommand
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}