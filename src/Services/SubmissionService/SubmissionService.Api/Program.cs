using SubmissionService.Application.Commands;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Abstractions;
using SubmissionService.Application.Queries;
using SubmissionService.Application.UseCases;
using SubmissionService.Infrastructure.Data;
using SubmissionService.Infrastructure.Repositories;
using SubmissionService.Infrastructure.Clients;
using Common.Application.Abstractions;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SubmissionService.Api;
using SubmissionService.Application.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<SubmissionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();

// HTTP Clients
builder.Services.AddHttpClient<IExamServiceClient, ExamServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ExamService"] ?? "http://localhost:5002");
});

// Use Cases
builder.Services.AddScoped<ICommandHandler<SubmitExamCommand, SubmitExamResult>, SubmitExamUseCase>();
builder.Services.AddScoped<IQueryHandler<GetSubmissionQuery, SubmissionDto>, GetSubmissionUseCase>();
builder.Services.AddScoped<IQueryHandler<GetUserSubmissionsQuery, IEnumerable<SubmissionDto>>, GetUserSubmissionsUseCase>();

// Infrastructure
builder.Services.AddRabbitMessageQueue(builder.Configuration);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.RequireHttpsMetadata = false;
        options.Audience = "submission-api";
    });

// Add resilience services
builder.Services.AddResilience(builder.Configuration);

// Add health checks
builder.Services.AddCustomHealthChecks<SubmissionDbContext>(builder.Configuration);

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