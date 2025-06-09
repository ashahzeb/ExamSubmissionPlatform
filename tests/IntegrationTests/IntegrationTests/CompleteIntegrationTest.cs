using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using AuthService.Infrastructure.Data;
using ExamService.Infrastructure.Data;
using SubmissionService.Infrastructure.Data;
using AuthService.Application.Commands;
using AuthService.Application.Results;
using ExamService.Application.Commands;
using ExamService.Application.DTOs;
using SubmissionService.Application.Requests;
using SubmissionService.Application.Results;
using SubmissionService.Application.Abstractions;
using Common.Domain.Abstractions;
using Common.Application.Abstractions;
using FluentAssertions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AuthService.Domain.Entities;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;
using Moq;
using Testcontainers.MsSql;

namespace IntegrationTests;

public class CompleteIntegrationTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly JsonSerializerOptions _jsonOptions;
    private MsSqlContainer _sqlServer;
    private string _connectionString;
    
    private WebApplicationFactory<AuthService.Api.Controllers.AuthController> _authFactory;
    private WebApplicationFactory<ExamService.Api.Controllers.ExamController> _examFactory;
    private WebApplicationFactory<SubmissionService.Api.Controllers.SubmissionController> _submissionFactory;
    private HttpClient _authClient;
    private HttpClient _examClient;
    private HttpClient _submissionClient;

    public CompleteIntegrationTest(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task InitializeAsync()
    {
        _output.WriteLine("Starting SQL Server container...");
        
        _sqlServer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("StrongPassword123!")
            .WithPortBinding(11433, 1433)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .WithStartupCallback((container, ct) =>
            {
                _output.WriteLine($"Container started: {container.Id}");
                return Task.CompletedTask;
            })
            .Build();

        try 
        {
            await _sqlServer.StartAsync();
            var baseConnectionString = _sqlServer.GetConnectionString();
            _connectionString = baseConnectionString.Replace("Database=master", "Database=IntegrationTestDb");
            _output.WriteLine($"✅ SQL Server container started successfully");
            _output.WriteLine($"Connection string: {_connectionString}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Failed to start SQL Server container: {ex.Message}");
            throw;
        }

        await SetupServices();
        await InitializeDatabases();
    }

    private async Task SetupServices()
    {
        // Setup Auth Service
        _authFactory = new WebApplicationFactory<AuthService.Api.Controllers.AuthController>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    RemoveDbContext<AuthDbContext>(services);
                    
                    // Add SQL Server DbContext
                    services.AddDbContext<AuthDbContext>(options =>
                    {
                        options.UseSqlServer(_connectionString);
                        options.EnableSensitiveDataLogging();
                    });
                    
                    MockExternalServices(services);
                });
            });
        _authClient = _authFactory.CreateClient();

        // Setup Exam Service
        _examFactory = new WebApplicationFactory<ExamService.Api.Controllers.ExamController>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    RemoveDbContext<ExamDbContext>(services);
                    
                    services.AddDbContext<ExamDbContext>(options =>
                    {
                        options.UseSqlServer(_connectionString);
                        options.EnableSensitiveDataLogging();
                    });
                    
                    MockExternalServices(services);
                    ConfigureTestAuthentication(services);
                });
            });
        _examClient = _examFactory.CreateClient();

        // Setup Submission Service
        _submissionFactory = new WebApplicationFactory<SubmissionService.Api.Controllers.SubmissionController>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    RemoveDbContext<SubmissionDbContext>(services);
                    
                    services.AddDbContext<SubmissionDbContext>(options =>
                    {
                        options.UseSqlServer(_connectionString);
                        options.EnableSensitiveDataLogging();
                    });
                    
                    MockExternalServices(services);
                    ConfigureTestAuthentication(services);
                    
                    // Replace ExamServiceClient
                    var clientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IExamServiceClient));
                    if (clientDescriptor != null) services.Remove(clientDescriptor);
                    services.AddHttpContextAccessor();
                    services.AddScoped<IExamServiceClient>(provider => 
                        new TestExamServiceClient(_examClient, provider.GetRequiredService<IHttpContextAccessor>()));
                });
            });
        _submissionClient = _submissionFactory.CreateClient();
    }

    [Fact]
    public async Task CompleteUserJourney_RegisterLoginCreateExamSubmit_ShouldWork()
    {
        _output.WriteLine("Starting Complete Integration Test");

        // Step 1: Register User
        var registerCommand = new RegisterCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "SecurePass123!"
        };

        var registerResponse = await _authClient.PostAsync("/api/auth/register", CreateJsonContent(registerCommand));
        registerResponse.Should().BeSuccessful();
        
        var registerResult = await DeserializeResponse<RegisterResult>(registerResponse);
        registerResult.IsSuccess.Should().BeTrue();
        var userId = registerResult.UserId;
        
        _output.WriteLine($"✅ User registered: {userId}");

        // Step 2: Login User
        var loginCommand = new LoginCommand
        {
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        var loginResponse = await _authClient.PostAsync("/api/auth/login", CreateJsonContent(loginCommand));
        loginResponse.Should().BeSuccessful();
        
        var loginResult = await DeserializeResponse<LoginResult>(loginResponse);
        loginResult.Token.Should().NotBeNullOrEmpty();
        var token = loginResult.Token;
        
        _output.WriteLine("✅ User logged in and token received");

        // Step 3: Create Exam
        var createExamCommand = new CreateExamCommand
        {
            Title = "Integration Test Exam",
            Description = "Test exam for integration testing",
            StartTime = DateTime.UtcNow.AddMinutes(-15),
            EndTime = DateTime.UtcNow.AddHours(2),
            TimeZone = "UTC",
            DurationMinutes = 120,
            MaxAttempts = 3,
            CreatedByUserId = userId
        };

        _examClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createExamResponse = await _examClient.PostAsync("/api/exam", CreateJsonContent(createExamCommand));
        createExamResponse.Should().BeSuccessful();
        
        var createExamResult = await DeserializeResponse<CreateExamResult>(createExamResponse);
        createExamResult.IsSuccess.Should().BeTrue();
        var examId = createExamResult.ExamId;
        
        _output.WriteLine($"✅ Exam created: {examId}");

        // Step 4: Verify Exam
        var getExamResponse = await _examClient.GetAsync($"/api/exam/{examId}");
        getExamResponse.Should().BeSuccessful();
        
        var examDto = await DeserializeResponse<ExamDto>(getExamResponse);
        examDto.Id.Should().Be(examId);
        examDto.Title.Should().Be("Integration Test Exam");
        
        _output.WriteLine("✅ Exam verified");

        // Step 5: Submit to Exam
        var submitRequest = new SubmitExamRequest
        {
            ExamId = examId,
            Content = "This is my test submission content",
            StartedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _submissionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var submitResponse = await _submissionClient.PostAsync("/api/submission/submit", CreateJsonContent(submitRequest));
        submitResponse.Should().BeSuccessful();
        
        var submitResult = await DeserializeResponse<SubmitExamResult>(submitResponse);
        submitResult.IsSuccess.Should().BeTrue();
        submitResult.AttemptNumber.Should().Be(1);
        var submissionId = submitResult.SubmissionId;
        
        _output.WriteLine($"✅ Submission created: {submissionId}");

        // Step 6: Verify Submission
        var getSubmissionResponse = await _submissionClient.GetAsync($"/api/submission/{submissionId}");
        getSubmissionResponse.Should().BeSuccessful();
        
        var submissionDto = await DeserializeResponse<SubmissionService.Application.DTOs.SubmissionDto>(getSubmissionResponse);
        submissionDto.Id.Should().Be(submissionId);
        submissionDto.ExamId.Should().Be(examId);
        submissionDto.UserId.Should().Be(userId);
        submissionDto.Content.Should().Be("This is my test submission content");
        submissionDto.AttemptNumber.Should().Be(1);
        
        _output.WriteLine("✅ Submission verified");

        // Step 7: Submit Second Attempt
        var secondSubmitRequest = new SubmitExamRequest
        {
            ExamId = examId,
            Content = "This is my second submission",
            StartedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var secondSubmitResponse = await _submissionClient.PostAsync("/api/submission/submit", CreateJsonContent(secondSubmitRequest));
        secondSubmitResponse.Should().BeSuccessful();
        
        var secondSubmitResult = await DeserializeResponse<SubmitExamResult>(secondSubmitResponse);
        secondSubmitResult.IsSuccess.Should().BeTrue();
        secondSubmitResult.AttemptNumber.Should().Be(2);
        
        _output.WriteLine("✅ Second submission created");

        // Step 8: Get User Submissions
        var userSubmissionsResponse = await _submissionClient.GetAsync($"/api/submission/user/{userId}");
        userSubmissionsResponse.Should().BeSuccessful();
        
        var userSubmissions = await DeserializeResponse<List<SubmissionService.Application.DTOs.SubmissionDto>>(userSubmissionsResponse);
        userSubmissions.Should().HaveCount(2);
        userSubmissions.Should().OnlyContain(s => s.UserId == userId);
        userSubmissions.Should().OnlyContain(s => s.ExamId == examId);
        
        _output.WriteLine("✅ User submissions verified");

        _output.WriteLine("🎉 Complete Integration Test PASSED!");
    }
    
    [Fact]
    public async Task SubmitExam_WhenMaxAttemptsExceeded_ShouldFail()
    {
        var (userId, token) = await RegisterAndLoginUser("maxtest@example.com");
        var examId = await CreateExam(token, userId, maxAttempts: 1);

        var firstSubmit = await SubmitToExam(token, examId, "First submission");
        firstSubmit.IsSuccess.Should().BeTrue();

        var secondSubmit = await SubmitToExam(token, examId, "Second submission");
        secondSubmit.IsSuccess.Should().BeFalse();
        secondSubmit.Message.Should().Contain("Maximum attempts");
    }
    
    [Fact]
    public async Task SubmitExam_WhenExamExpired_ShouldFail()
    {
        // Setup
        var (userId, token) = await RegisterAndLoginUser("expired@example.com");
        
        // Create expired exam
        var expiredExamCommand = new CreateExamCommand
        {
            Title = "Expired Exam",
            Description = "This exam has ended",
            StartTime = DateTime.UtcNow.AddHours(-3),
            EndTime = DateTime.UtcNow.AddHours(-1), // Ended 1 hour ago
            TimeZone = "UTC",
            DurationMinutes = 60,
            MaxAttempts = 1,
            CreatedByUserId = userId
        };

        _examClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var examResponse = await _examClient.PostAsync("/api/exam", CreateJsonContent(expiredExamCommand));
        var examResult = await DeserializeResponse<CreateExamResult>(examResponse);

        // Try to submit
        var submitResult = await SubmitToExam(token, examResult.ExamId, "Late submission");
        submitResult.IsSuccess.Should().BeFalse();
        submitResult.Message.Should().Contain("deadline");

        _output.WriteLine("✅ Expired exam validation working correctly");
    }

    // Helper Methods
    private async Task<(Guid userId, string token)> RegisterAndLoginUser(string email)
    {
        var registerCommand = new RegisterCommand
        {
            Email = email,
            FirstName = "Test",
            LastName = "User", 
            Password = "SecurePass123!"
        };

        var registerResponse = await _authClient.PostAsync("/api/auth/register", CreateJsonContent(registerCommand));
        var registerResult = await DeserializeResponse<RegisterResult>(registerResponse);

        var loginCommand = new LoginCommand { Email = email, Password = "SecurePass123!" };
        var loginResponse = await _authClient.PostAsync("/api/auth/login", CreateJsonContent(loginCommand));
        var loginResult = await DeserializeResponse<LoginResult>(loginResponse);

        return (registerResult.UserId, loginResult.Token);
    }

    private async Task<Guid> CreateExam(string token, Guid userId, int maxAttempts = 3)
    {
        var createExamCommand = new CreateExamCommand
        {
            Title = "Test Exam",
            Description = "Test Description",
            StartTime = DateTime.UtcNow.AddMinutes(-30),
            EndTime = DateTime.UtcNow.AddHours(2),
            TimeZone = "UTC",
            DurationMinutes = 120,
            MaxAttempts = maxAttempts,
            CreatedByUserId = userId
        };

        _examClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _examClient.PostAsync("/api/exam", CreateJsonContent(createExamCommand));
        var result = await DeserializeResponse<CreateExamResult>(response);
        return result.ExamId;
    }

    private async Task<SubmitExamResult> SubmitToExam(string token, Guid examId, string content)
    {
        var submitRequest = new SubmitExamRequest
        {
            ExamId = examId,
            Content = content,
            StartedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _submissionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _submissionClient.PostAsync("/api/submission/submit", CreateJsonContent(submitRequest));
        return await DeserializeResponse<SubmitExamResult>(response);
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null) services.Remove(descriptor);
        
        var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
        if (contextDescriptor != null) services.Remove(contextDescriptor);
    }

    private static void MockExternalServices(IServiceCollection services)
    {
        // Create a proper mock for IRetryService that actually executes the operation
        var retryServiceMock = new Mock<IRetryService>();
        
        // Mock the generic ExecuteAsync<T> method
        retryServiceMock
            .Setup(service => service.ExecuteAsync(It.IsAny<Func<Task<int>>>(), It.IsAny<int>()))
            .Returns<Func<Task<int>>, int?>(async (func, retries) => await func());
            
        // Mock the non-generic ExecuteAsync method if needed
        retryServiceMock
            .Setup(service => service.ExecuteAsync(It.IsAny<Func<Task>>(), It.IsAny<int>()))
            .Returns<Func<Task>, int?>(async (func, retries) => await func());
            
        services.AddScoped(_ => retryServiceMock.Object);
        services.AddScoped(_ => Mock.Of<IEventPublisher>());
        services.AddScoped(_ => Mock.Of<ICacheService>());
    }
    
    private static void ConfigureTestAuthentication(IServiceCollection services)
    {
        services.AddAuthentication("TestBearer")
            .AddScheme<AuthenticationSchemeOptions, TestBearerHandler>("TestBearer", options => { });
        services.AddAuthorization();
    }
    
    private async Task InitializeDatabases()
    {
        _output.WriteLine("Initializing databases...");
        
        try
        {
            // Migrate Auth Database
            using var authScope = _authFactory.Services.CreateScope();
            var authContext = authScope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await authContext.Database.MigrateAsync();
            _output.WriteLine("✅ Auth database migrated");

            // Migrate Exam Database  
            using var examScope = _examFactory.Services.CreateScope();
            var examContext = examScope.ServiceProvider.GetRequiredService<ExamDbContext>();
            await examContext.Database.MigrateAsync();
            _output.WriteLine("✅ Exam database migrated");

            // Migrate Submission Database
            using var submissionScope = _submissionFactory.Services.CreateScope();
            var submissionContext = submissionScope.ServiceProvider.GetRequiredService<SubmissionDbContext>();
            await submissionContext.Database.MigrateAsync();
            _output.WriteLine("✅ Submission database migrated");
            
            _output.WriteLine("✅ All databases initialized successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Database initialization failed: {ex.Message}");
            throw;
        }
    }

    private static StringContent CreateJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
    }

    private async Task DebugUserInDatabase(string email)
    {
        _output.WriteLine($"🔍 Debugging user existence for: {email}");
        
        using var scope = _authFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        
        // Check total user count
        var totalUsers = await context.Users.CountAsync();
        _output.WriteLine($"Total users in database: {totalUsers}");
        
        // List all users
        var allUsers = await context.Users.ToListAsync();
        foreach (var user in allUsers)
        {
            _output.WriteLine($"User found: {user.Email}, ID: {user.Id}, Active: {user.IsActive}");
        }
        
        // Check specific user
        var targetUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        _output.WriteLine($"Target user ({email}) exists: {targetUser != null}");
        
        if (targetUser != null)
        {
            _output.WriteLine($"Target user details: ID={targetUser.Id}, Email={targetUser.Email}, Active={targetUser.IsActive}, Created={targetUser.CreatedAt}");
        }
        
        // Test direct SQL query
        try
        {
            var sqlResult = await context.Database.SqlQueryRaw<int>($"SELECT COUNT(*) as Value FROM Users WHERE Email = '{email}'").FirstAsync();
            _output.WriteLine($"Direct SQL count for {email}: {sqlResult}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"SQL query failed: {ex.Message}");
        }
    }
    
    public async Task DisposeAsync()
    {
        _output.WriteLine("Disposing resources...");
        
        _authClient?.Dispose();
        _examClient?.Dispose();
        _submissionClient?.Dispose();
        _authFactory?.Dispose();
        _examFactory?.Dispose();
        _submissionFactory?.Dispose();
        
        if (_sqlServer != null)
        {
            await _sqlServer.DisposeAsync();
            _output.WriteLine("✅ SQL Server container disposed");
        }
    }
}