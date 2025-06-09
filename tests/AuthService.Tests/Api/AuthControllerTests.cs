using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using AuthService.Api.Controllers;
using AuthService.Application.Commands;
using AuthService.Application.Results;
using Common.Application.Abstractions;
using Xunit;

namespace AuthService.Tests.Api;

public class AuthControllerTests
{
    private readonly Mock<ICommandHandler<LoginCommand, LoginResult>> _mockLoginHandler;
    private readonly Mock<ICommandHandler<RegisterCommand, RegisterResult>> _mockRegisterHandler;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockLoginHandler = new Mock<ICommandHandler<LoginCommand, LoginResult>>();
        _mockRegisterHandler = new Mock<ICommandHandler<RegisterCommand, RegisterResult>>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(_mockLoginHandler.Object, _mockRegisterHandler.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "password123" };
        var loginResult = new LoginResult 
        { 
            Token = "jwt-token", 
            UserId = Guid.NewGuid(), 
            Email = command.Email,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockLoginHandler.Setup(x => x.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(loginResult);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "wrongpassword" };

        _mockLoginHandler.Setup(x => x.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new RegisterCommand { Email = "test@example.com", Password = "SecurePass123!" };
        var registerResult = new RegisterResult 
        { 
            UserId = Guid.NewGuid(), 
            Email = command.Email, 
            IsSuccess = true,
            Message = "User registered successfully"
        };

        _mockRegisterHandler.Setup(x => x.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registerResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(registerResult);
    }

    [Fact]
    public async Task Register_WithFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new RegisterCommand { Email = "test@example.com", Password = "weak" };
        var registerResult = new RegisterResult 
        { 
            IsSuccess = false,
            Message = "Password does not meet security requirements"
        };

        _mockRegisterHandler.Setup(x => x.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registerResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}