using ExamService.Application.Commands;
using ExamService.Application.DTOs;
using ExamService.Application.Queries;
using Common.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExamService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamController(
    ICommandHandler<CreateExamCommand, CreateExamResult> createExamHandler,
    IQueryHandler<GetExamQuery, ExamDto> getExamHandler,
    IQueryHandler<GetActiveExamsQuery, IEnumerable<ExamDto>> getActiveExamsHandler,
    IQueryHandler<CheckExamExistsQuery, bool> checkExamExistsHandler,
    ILogger<ExamController> logger)
    : ControllerBase
{
    private readonly ICommandHandler<CreateExamCommand, CreateExamResult> _createExamHandler = createExamHandler ?? throw new ArgumentNullException(nameof(createExamHandler));
    private readonly IQueryHandler<GetExamQuery, ExamDto> _getExamHandler = getExamHandler ?? throw new ArgumentNullException(nameof(getExamHandler));
    private readonly IQueryHandler<GetActiveExamsQuery, IEnumerable<ExamDto>> _getActiveExamsHandler = getActiveExamsHandler ?? throw new ArgumentNullException(nameof(getActiveExamsHandler));
    private readonly IQueryHandler<CheckExamExistsQuery, bool> _checkExamExistsHandler = checkExamExistsHandler ?? throw new ArgumentNullException(nameof(checkExamExistsHandler));
    private readonly ILogger<ExamController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpPost]
    public async Task<ActionResult<CreateExamResult>> CreateExam([FromBody] CreateExamCommand command)
    {
        try
        {
            command.CreatedByUserId = GetCurrentUserId();
            var result = await _createExamHandler.HandleAsync(command);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetExam), new { id = result.ExamId }, result);
            }
            
            return BadRequest(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid exam creation request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exam");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExamDto>> GetExam(Guid id)
    {
        try
        {
            var query = new GetExamQuery { ExamId = id };
            var exam = await _getExamHandler.HandleAsync(query);
            return Ok(exam);
        }
        catch (Common.Domain.Exceptions.NotFoundException ex)
        {
            _logger.LogWarning(ex, "Exam not found: {ExamId}", id);
            return NotFound(new { message = "Exam not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exam: {ExamId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetActiveExams(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetActiveExamsQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100) // Limit page size
            };

            var exams = await _getActiveExamsHandler.HandleAsync(query);
            return Ok(exams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active exams");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id:guid}/exists")]
    public async Task<ActionResult<bool>> ExamExists(Guid id)
    {
        try
        {
            var query = new CheckExamExistsQuery { ExamId = id };
            var exists = await _checkExamExistsHandler.HandleAsync(query);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if exam exists: {ExamId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ExamService", timestamp = DateTime.UtcNow });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}