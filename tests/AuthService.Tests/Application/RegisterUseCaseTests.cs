using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AuthService.Application.Abstractions;
using AuthService.Application.Commands;
using AuthService.Application.UseCases;
using AuthService.Domain.Entities;
using Common.Application.Abstractions;
using Common.Contracts.Events;
using Xunit;

namespace AuthService.Tests.Application;

public class RegisterUseCaseTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly Mock<ILogger<RegisterUseCase>> _mockLogger;
    private readonly RegisterUseCase _useCase;

    public RegisterUseCaseTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _mockLogger = new Mock<ILogger<RegisterUseCase>>();

        _useCase = new RegisterUseCase(
            _mockUserRepository.Object,
            _mockPasswordService.Object,
            _mockEventPublisher.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldRegisterUser()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            Email = "test@example.com", 
            FirstName = "John", 
            LastName = "Doe", 
            Password = "SecurePass123!" 
        };

        _mockUserRepository.Setup(x => x.ExistsAsync(command.Email))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsPasswordStrong(command.Password))
            .Returns(true);
        _mockPasswordService.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedpassword");

        // Act
        var result = await _useCase.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.UserId.Should().NotBe(Guid.Empty);
        result.Email.Should().Be(command.Email);
        result.Message.Should().Be("User registered successfully");

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand { Email = "existing@example.com", Password = "SecurePass123!" };

        _mockUserRepository.Setup(x => x.ExistsAsync(command.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User with this email already exists");

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithWeakPassword_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand { Email = "test@example.com", Password = "weak" };

        _mockUserRepository.Setup(x => x.ExistsAsync(command.Email))
            .ReturnsAsync(false);
        _mockPasswordService.Setup(x => x.IsPasswordStrong(command.Password))
            .Returns(false);

        // Act
        var result = await _useCase.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Password does not meet security requirements");

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }
}
