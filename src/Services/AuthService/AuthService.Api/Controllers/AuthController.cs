using AuthService.Application.Commands;
using AuthService.Application.Results;
using Common.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, LoginResult> _loginHandler;
    private readonly ICommandHandler<RegisterCommand, RegisterResult> _registerHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ICommandHandler<LoginCommand, LoginResult> loginHandler,
        ICommandHandler<RegisterCommand, RegisterResult> registerHandler,
        ILogger<AuthController> logger)
    {
        _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));
        _registerHandler = registerHandler ?? throw new ArgumentNullException(nameof(registerHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _loginHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed for email: {Email}", command.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", command.Email);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var result = await _registerHandler.HandleAsync(command);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetUser), new { id = result.UserId }, result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", command.Email);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("user/{id:guid}")]
    public async Task<ActionResult> GetUser(Guid id)
    {
        // This would typically use a query handler
        return Ok(new { id, message = "User endpoint - implement with query handler" });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "AuthService", timestamp = DateTime.UtcNow });
    }
}