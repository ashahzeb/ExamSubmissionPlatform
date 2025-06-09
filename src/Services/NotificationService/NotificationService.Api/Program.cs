using System.Text;
using NotificationService.Application.Commands;
using NotificationService.Application.EventHandlers;
using NotificationService.Application.Abstractions;
using NotificationService.Application.UseCases;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Clients;
using Common.Application.Abstractions;
using Common.Contracts.Events;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Application.Results;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog logging
builder.Host.UseSerilogLogging(builder.Configuration);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// HTTP Clients
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthService"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IExamServiceClient, ExamServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ExamService"] ?? "http://localhost:5002");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Notification Services
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SmsNotificationService>();
builder.Services.AddScoped<InAppNotificationService>();

builder.Services.AddScoped<INotificationService, EmailNotificationService>();

// Use Cases
builder.Services.AddScoped<ICommandHandler<SendNotificationCommand, SendNotificationResult>, SendNotificationUseCase>();

// Event Handlers - This service CONSUMES events from other services
builder.Services.AddScoped<IEventHandler<ExamSubmittedEvent>, ExamSubmittedEventHandler>();
builder.Services.AddEventConsumer<ExamSubmittedEvent>();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCustomDbContext<NotificationDbContext>(builder.Configuration);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = "ExamPlatform",
            ValidateAudience = true,
            ValidAudience = "ExamPlatform",  
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("d8a6362b248030d523c135efe3e15d5aed6111031ff2742d746e4d2c997d9b0f")), // this needs to be read from a common source or better use discovery endpoint 
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS configuration
builder.Services.AddApplicationCors(builder.Configuration, builder.Environment);

// Add health checks
builder.Services.AddCustomHealthChecks<NotificationDbContext>(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseInfrastructure();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.MigrateDatabase<NotificationDbContext>();
app.Run();

public partial class Program { }