using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AuthService.Infrastructure.Services;
using Xunit;

namespace AuthService.Tests.Infrastructure;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        var mockLogger = new Mock<ILogger<PasswordService>>();
        _passwordService = new PasswordService(mockLogger.Object);
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Length.Should().BeGreaterThan(50);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordService.HashPassword(password);

        // Act
        var isValid = _passwordService.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("SecurePass123!", true)]
    [InlineData("ComplexP@ssw0rd", true)]
    [InlineData("Weak123", false)]
    [InlineData("nouppernumber1!", false)]
    [InlineData("NOLOWERNUMBER1!", false)]
    [InlineData("NoNumbersUpper!", false)]
    [InlineData("NoSpecialChars123", false)]
    [InlineData("Short1!", false)]
    public void IsPasswordStrong_ShouldValidateCorrectly(string password, bool expected)
    {
        // Act
        var result = _passwordService.IsPasswordStrong(password);

        // Assert
        result.Should().Be(expected);
    }
}