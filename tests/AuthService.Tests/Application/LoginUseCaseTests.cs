using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AuthService.Application.Abstractions;
using AuthService.Application.Commands;
using AuthService.Application.UseCases;
using AuthService.Domain.Entities;
using Xunit;

namespace AuthService.Tests.Application;

public class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ILogger<LoginUseCase>> _mockLogger;
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<LoginUseCase>>();

        _useCase = new LoginUseCase(
            _mockUserRepository.Object,
            _mockPasswordService.Object,
            _mockJwtTokenService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnLoginResult()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "password123" };
        var user = User.Create("test@example.com", "John", "Doe", "hashedpassword");
        var tokenResult = new AuthService.Application.Results.TokenResult 
        { 
            Token = "jwt-token", 
            ExpiresAt = DateTime.UtcNow.AddHours(1) 
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _mockJwtTokenService.Setup(x => x.GenerateToken(user))
            .Returns(tokenResult);

        // Act
        var result = await _useCase.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(tokenResult.Token);
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.ExpiresAt.Should().Be(tokenResult.ExpiresAt);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand { Email = "nonexistent@example.com", Password = "password123" };
        
        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var act = async () => await _useCase.HandleAsync(command);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "wrongpassword" };
        var user = User.Create("test@example.com", "John", "Doe", "hashedpassword");

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);
        _mockPasswordService.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        var act = async () => await _useCase.HandleAsync(command);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand { Email = "test@example.com", Password = "password123" };
        var user = User.Create("test@example.com", "John", "Doe", "hashedpassword");
        user.Deactivate();

        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);

        // Act & Assert
        var act = async () => await _useCase.HandleAsync(command);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }
}