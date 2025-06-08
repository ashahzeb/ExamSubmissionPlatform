using AuthService.Application.Commands;
using AuthService.Application.Abstractions;
using AuthService.Application.Results;
using AuthService.Application.UseCases;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Common.Application.Abstractions;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Use Cases
builder.Services.AddScoped<ICommandHandler<LoginCommand, LoginResult>, LoginUseCase>();
builder.Services.AddScoped<ICommandHandler<RegisterCommand, RegisterResult>, RegisterUseCase>();

// Infrastructure
builder.Services.AddRabbitMessageQueue(builder.Configuration);

// Add resilience services
builder.Services.AddResilience(builder.Configuration);

// Add health checks
builder.Services.AddCustomHealthChecks<AuthDbContext>(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();