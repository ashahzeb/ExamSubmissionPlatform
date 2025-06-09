using System.Text;
using Common.Application.Abstractions;
using SubmissionService.Application.Commands;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Queries;
using SubmissionService.Application.UseCases;
using SubmissionService.Infrastructure.Data;
using SubmissionService.Infrastructure.Repositories;
using SubmissionService.Infrastructure.Clients;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Logging;
using Common.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SubmissionService.Application.Abstractions;
using SubmissionService.Application.Results;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog logging
builder.Host.UseSerilogLogging(builder.Configuration);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Repositories
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();

// HTTP Clients
builder.Services.AddHttpClient<IExamServiceClient, ExamServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ExamService"] ?? "http://localhost:8002");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Use Cases
builder.Services.AddScoped<ICommandHandler<SubmitExamCommand, SubmitExamResult>, SubmitExamUseCase>();
builder.Services.AddScoped<IQueryHandler<GetSubmissionQuery, SubmissionDto>, GetSubmissionUseCase>();
builder.Services.AddScoped<IQueryHandler<GetUserSubmissionsQuery, IEnumerable<SubmissionDto>>, GetUserSubmissionsUseCase>();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCustomDbContext<SubmissionDbContext>(builder.Configuration);

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
builder.Services.AddCustomHealthChecks<SubmissionDbContext>(builder.Configuration);

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

await app.MigrateDatabase<SubmissionDbContext>();
app.Run();

public partial class Program { }