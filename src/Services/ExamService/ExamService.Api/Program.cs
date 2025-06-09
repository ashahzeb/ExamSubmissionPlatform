using System.Text;
using ExamService.Application.Commands;
using ExamService.Application.DTOs;
using ExamService.Application.Abstractions;
using ExamService.Application.Queries;
using ExamService.Application.UseCases;
using ExamService.Infrastructure.Data;
using ExamService.Infrastructure.Repositories;
using Common.Application.Abstractions;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Logging;
using Common.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog logging
builder.Host.UseSerilogLogging(builder.Configuration);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories
builder.Services.AddScoped<IExamRepository, ExamRepository>();

// Services
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();

// Use Cases
builder.Services.AddScoped<ICommandHandler<CreateExamCommand, CreateExamResult>, CreateExamUseCase>();
builder.Services.AddScoped<IQueryHandler<GetExamQuery, ExamDto>, GetExamUseCase>();
builder.Services.AddScoped<IQueryHandler<GetActiveExamsQuery, IEnumerable<ExamDto>>, GetActiveExamsUseCase>();
builder.Services.AddScoped<IQueryHandler<CheckExamExistsQuery, bool>, CheckExamExistsUseCase>();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCustomDbContext<ExamDbContext>(builder.Configuration);

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
builder.Services.AddCustomHealthChecks<ExamDbContext>(builder.Configuration);

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

await app.MigrateDatabase<ExamDbContext>();
app.Run();

public partial class Program { }