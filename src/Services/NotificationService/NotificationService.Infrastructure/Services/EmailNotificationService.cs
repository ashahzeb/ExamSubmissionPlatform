using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
    : INotificationService
{
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly ILogger<EmailNotificationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> SendAsync(Notification notification)
    {
        if (notification.Type != NotificationType.Email)
        {
            return false;
        }

        try
        {
            var smtpSettings = _configuration.GetSection("Smtp");
            
            using var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
            };

            var message = new MailMessage
            {
                From = new MailAddress(smtpSettings["FromAddress"] ?? "noreply@examplatform.com", "Exam Platform"),
                Subject = notification.Title,
                Body = notification.Message,
                IsBodyHtml = true
            };

            message.To.Add(notification.Recipient);

            await client.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {Recipient}", notification.Recipient);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", notification.Recipient);
            return false;
        }
    }
}