using ExamService.Application.Commands;
using ExamService.Application.DTOs;
using ExamService.Application.Abstractions;
using ExamService.Application.Queries;
using ExamService.Application.UseCases;
using ExamService.Infrastructure.Data;
using ExamService.Infrastructure.Repositories;
using ExamService.Infrastructure.Services;
using Common.Application.Abstractions;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ExamDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddRabbitMessageQueue(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.RequireHttpsMetadata = false;
        options.Audience = "exam-api";
    });

// Add resilience services
builder.Services.AddResilience(builder.Configuration);

// Add health checks
builder.Services.AddCustomHealthChecks<ExamDbContext>(builder.Configuration);

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