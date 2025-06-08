namespace NotificationService.Application.Results;

public class SendNotificationResult
{
    public Guid NotificationId { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static SendNotificationResult Success(Guid notificationId)
    {
        return new SendNotificationResult
        {
            NotificationId = notificationId,
            IsSuccess = true,
            Message = "Notification sent successfully"
        };
    }

    public static SendNotificationResult Failure(string message)
    {
        return new SendNotificationResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}