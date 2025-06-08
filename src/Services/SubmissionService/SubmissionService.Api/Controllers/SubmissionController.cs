using SubmissionService.Application.Commands;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Queries;
using Common.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SubmissionService.Application.Requests;
using SubmissionService.Application.Results;

namespace SubmissionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionController : ControllerBase
{
    private readonly ICommandHandler<SubmitExamCommand, SubmitExamResult> _submitExamHandler;
    private readonly IQueryHandler<GetSubmissionQuery, SubmissionDto> _getSubmissionHandler;
    private readonly IQueryHandler<GetUserSubmissionsQuery, IEnumerable<SubmissionDto>> _getUserSubmissionsHandler;
    private readonly ILogger<SubmissionController> _logger;

    public SubmissionController(
        ICommandHandler<SubmitExamCommand, SubmitExamResult> submitExamHandler,
        IQueryHandler<GetSubmissionQuery, SubmissionDto> getSubmissionHandler,
        IQueryHandler<GetUserSubmissionsQuery, IEnumerable<SubmissionDto>> getUserSubmissionsHandler,
        ILogger<SubmissionController> logger)
    {
        _submitExamHandler = submitExamHandler ?? throw new ArgumentNullException(nameof(submitExamHandler));
        _getSubmissionHandler = getSubmissionHandler ?? throw new ArgumentNullException(nameof(getSubmissionHandler));
        _getUserSubmissionsHandler = getUserSubmissionsHandler ?? throw new ArgumentNullException(nameof(getUserSubmissionsHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("submit")]
    public async Task<ActionResult<SubmitExamResult>> SubmitExam([FromBody] SubmitExamRequest request)
    {
        try
        {
            var command = new SubmitExamCommand
            {
                ExamId = request.ExamId,
                UserId = GetCurrentUserId(),
                Content = request.Content,
                StartedAt = request.StartedAt,
                UserAgent = Request.Headers.UserAgent.ToString(),
                IpAddress = GetClientIpAddress()
            };

            var result = await _submitExamHandler.HandleAsync(command);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetSubmission), 
                    new { id = result.SubmissionId }, result);
            }
            
            return BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting exam for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> GetSubmission(Guid id)
    {
        try
        {
            var query = new GetSubmissionQuery { SubmissionId = id };
            var submission = await _getSubmissionHandler.HandleAsync(query);
            
            // Check if user owns this submission or is authorized to view it
            if (submission.UserId != GetCurrentUserId() && !IsAdminUser())
            {
                return Forbid();
            }
            
            return Ok(submission);
        }
        catch (Common.Domain.Exceptions.NotFoundException ex)
        {
            _logger.LogWarning(ex, "Submission not found: {SubmissionId}", id);
            return NotFound(new { message = "Submission not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving submission: {SubmissionId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<SubmissionDto>>> GetUserSubmissions(
        Guid userId,
        [FromQuery] Guid? examId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Users can only view their own submissions unless they're admin
            if (userId != GetCurrentUserId() && !IsAdminUser())
            {
                return Forbid();
            }

            var query = new GetUserSubmissionsQuery
            {
                UserId = userId,
                ExamId = examId,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100)
            };

            var submissions = await _getUserSubmissionsHandler.HandleAsync(query);
            return Ok(submissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user submissions for user: {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "SubmissionService", timestamp = DateTime.UtcNow });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private bool IsAdminUser()
    {
        return User.IsInRole("Admin") || User.HasClaim("role", "admin");
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}