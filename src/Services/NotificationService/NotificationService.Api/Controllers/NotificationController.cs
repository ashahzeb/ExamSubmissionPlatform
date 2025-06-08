using NotificationService.Application.Commands;
using Common.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Results;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController(
    ICommandHandler<SendNotificationCommand, SendNotificationResult> sendNotificationHandler,
    ILogger<NotificationController> logger)
    : ControllerBase
{
    private readonly ICommandHandler<SendNotificationCommand, SendNotificationResult> _sendNotificationHandler = sendNotificationHandler ?? throw new ArgumentNullException(nameof(sendNotificationHandler));
    private readonly ILogger<NotificationController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost("send")]
    public async Task<ActionResult<SendNotificationResult>> SendNotification([FromBody] SendNotificationCommand command)
    {
        try
        {
            var result = await _sendNotificationHandler.HandleAsync(command);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "NotificationService", timestamp = DateTime.UtcNow });
    }
}