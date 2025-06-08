using Common.Domain;
using Common.Domain.Entities;

namespace NotificationService.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public string Recipient { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? FailureReason { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; }

    private Notification() 
    { 
        Metadata = new Dictionary<string, object>();
    }

    private Notification(Guid userId, NotificationType type, string title, 
        string message, string recipient)
    {
        UserId = userId;
        Type = type;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
        Status = NotificationStatus.Pending;
        RetryCount = 0;
        Metadata = new Dictionary<string, object>();
    }

    public static Notification Create(Guid userId, NotificationType type, 
        string title, string message, string recipient)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient cannot be empty", nameof(recipient));

        return new Notification(userId, type, title, message, recipient);
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Sent)
        {
            Status = NotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }

    public void MarkAsFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;
        RetryCount++;
        UpdateTimestamp();
    }

    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
        UpdateTimestamp();
    }

    public bool CanRetry(int maxRetries = 3)
    {
        return Status == NotificationStatus.Failed && RetryCount < maxRetries;
    }
}

public enum NotificationType
{
    Email = 0,
    Sms = 1,
    Push = 2,
    InApp = 3
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Read = 2,
    Failed = 3
}