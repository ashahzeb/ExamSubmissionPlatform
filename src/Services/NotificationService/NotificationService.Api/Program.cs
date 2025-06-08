using NotificationService.Application.Commands;
using NotificationService.Application.EventHandlers;
using NotificationService.Application.Abstractions;
using NotificationService.Application.UseCases;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Clients;
using NotificationService.Api.BackgroundServices;
using Common.Application.Abstractions;
using Common.Contracts.Events;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotificationService.Application.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// HTTP Clients
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthService"] ?? "http://localhost:5001");
});

builder.Services.AddHttpClient<IExamServiceClient, ExamServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ExamService"] ?? "http://localhost:5002");
});

// Notification Services
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<SmsNotificationService>();
builder.Services.AddScoped<InAppNotificationService>();
builder.Services.AddScoped<INotificationService, CompositeNotificationService>();

// Use Cases
builder.Services.AddScoped<ICommandHandler<SendNotificationCommand, SendNotificationResult>, SendNotificationUseCase>();

// Event Handlers
builder.Services.AddScoped<IEventHandler<ExamSubmittedEvent>, ExamSubmittedEventHandler>();

// Background Services
builder.Services.AddHostedService<NotificationProcessorService>();

// Infrastructure
builder.Services.AddRabbitMessageQueue(builder.Configuration);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.RequireHttpsMetadata = false;
        options.Audience = "notification-api";
    });

// Add resilience services
builder.Services.AddResilience(builder.Configuration);

// Add health checks
builder.Services.AddCustomHealthChecks<NotificationDbContext>(builder.Configuration);


var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();