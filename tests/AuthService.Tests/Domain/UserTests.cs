using AuthService.Domain.Entities;
using FluentAssertions;

namespace AuthService.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var passwordHash = "hashedpassword123";

        // Act
        var user = User.Create(email, firstName, lastName, passwordHash);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.PasswordHash.Should().Be(passwordHash);
        user.IsActive.Should().BeTrue();
        user.LastLoginAt.Should().BeNull();
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        var act = () => User.Create(email, "John", "Doe", "hash");
        act.Should().Throw<ArgumentException>().WithMessage("Email cannot be empty*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    [InlineData("test.com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        var act = () => User.Create(email, "John", "Doe", "hash");
        act.Should().Throw<ArgumentException>().WithMessage("Invalid email format*");
    }

    [Fact]
    public void UpdateLastLogin_ShouldSetLastLoginTime()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        user.UpdateLastLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
    }
}