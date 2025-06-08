using AuthService.Application.Commands;
using AuthService.Application.Abstractions;
using AuthService.Application.Results;
using AuthService.Application.UseCases;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Common.Application.Abstractions;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog logging
builder.Host.UseSerilogLogging(builder.Configuration);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Use Cases
builder.Services.AddScoped<ICommandHandler<LoginCommand, LoginResult>, LoginUseCase>();
builder.Services.AddScoped<ICommandHandler<RegisterCommand, RegisterResult>, RegisterUseCase>();

// Infrastructure (includes DbContext, RabbitMQ, Redis, etc.)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCustomDbContext<AuthDbContext>(builder.Configuration);

// Add health checks
builder.Services.AddCustomHealthChecks<AuthDbContext>(builder.Configuration);

// CORS configuration
builder.Services.AddApplicationCors(builder.Configuration, builder.Environment);

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
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.MigrateDatabase<AuthDbContext>();
app.Run();