using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("ApiRateLimit")]
public class GatewayController : ControllerBase
{
    private readonly ILogger<GatewayController> _logger;
    private readonly IConfiguration _configuration;

    public GatewayController(ILogger<GatewayController> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            service = "ApiGateway",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        });
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(new
        {
            name = "Exam Platform API Gateway",
            version = "1.0.0",
            description = "Central gateway for exam submission platform microservices",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            services = new[]
            {
                new { 
                    name = "AuthService", 
                    path = "/api/auth", 
                    upstream = _configuration["ReverseProxy:Clusters:auth-cluster:Destinations:destination1:Address"],
                    healthCheck = "/api/auth/health"
                },
                new { 
                    name = "ExamService", 
                    path = "/api/exam", 
                    upstream = _configuration["ReverseProxy:Clusters:exam-cluster:Destinations:destination1:Address"],
                    healthCheck = "/api/exam/health"
                },
                new { 
                    name = "SubmissionService", 
                    path = "/api/submission", 
                    upstream = _configuration["ReverseProxy:Clusters:submission-cluster:Destinations:destination1:Address"],
                    healthCheck = "/api/submission/health"
                },
                new { 
                    name = "NotificationService", 
                    path = "/api/notification", 
                    upstream = _configuration["ReverseProxy:Clusters:notification-cluster:Destinations:destination1:Address"],
                    healthCheck = "/api/notification/health"
                }
            },
            features = new[]
            {
                "JWT Authentication",
                "Rate Limiting",
                "Health Checks",
                "CORS Support",
                "Request Routing",
                "Load Balancing"
            }
        });
    }

    [HttpGet("routes")]
    public IActionResult Routes()
    {
        var routes = new[]
        {
            new { method = "POST", path = "/api/auth/register", description = "User registration" },
            new { method = "POST", path = "/api/auth/login", description = "User authentication" },
            new { method = "GET", path = "/api/auth/user/{id}", description = "Get user details" },
            new { method = "POST", path = "/api/exam", description = "Create exam" },
            new { method = "GET", path = "/api/exam/active", description = "Get active exams" },
            new { method = "GET", path = "/api/exam/{id}", description = "Get exam details" },
            new { method = "POST", path = "/api/submission/submit", description = "Submit exam" },
            new { method = "GET", path = "/api/submission/{id}", description = "Get submission" },
            new { method = "GET", path = "/api/submission/user/{userId}", description = "Get user submissions" },
            new { method = "POST", path = "/api/notification/send", description = "Send notification" },
            new { method = "GET", path = "/api/notification/user/{userId}", description = "Get user notifications" }
        };

        return Ok(new { routes });
    }
}