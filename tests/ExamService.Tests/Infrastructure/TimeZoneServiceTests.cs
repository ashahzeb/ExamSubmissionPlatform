using Common.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ExamService.Domain.Entities;

namespace ExamService.Tests.Infrastructure;

public class TimeZoneServiceTests
{
    private readonly TimeZoneService _timeZoneService;

    public TimeZoneServiceTests()
    {
        var mockLogger = new Mock<ILogger<TimeZoneService>>();
        _timeZoneService = new TimeZoneService(mockLogger.Object);
    }

    [Theory]
    [InlineData("UTC")]
    [InlineData("Eastern Standard Time")]
    [InlineData("Pacific Standard Time")]
    [InlineData("Central European Standard Time")]
    public void IsValidTimeZone_WithValidTimeZones_ShouldReturnTrue(string timeZoneId)
    {
        // Act
        var result = _timeZoneService.IsValidTimeZone(timeZoneId);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Invalid/TimeZone")]
    [InlineData("")]
    [InlineData("NotATimeZone")]
    public void IsValidTimeZone_WithInvalidTimeZones_ShouldReturnFalse(string timeZoneId)
    {
        // Act
        var result = _timeZoneService.IsValidTimeZone(timeZoneId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ConvertToUtc_WithValidTimeZone_ShouldConvertCorrectly()
    {
        // Arrange
        var localDateTime = new DateTime(2024, 6, 15, 14, 30, 0);
        var timeZoneId = "UTC";

        // Act
        var utcDateTime = _timeZoneService.ConvertToUtc(localDateTime, timeZoneId);

        // Assert
        utcDateTime.Should().Be(localDateTime); // UTC to UTC should be same
    }

    [Fact]
    public void ConvertToUtc_WithInvalidTimeZone_ShouldThrowArgumentException()
    {
        // Arrange
        var localDateTime = DateTime.Now;
        var invalidTimeZone = "Invalid/TimeZone";

        // Act & Assert
        var act = () => _timeZoneService.ConvertToUtc(localDateTime, invalidTimeZone);
        act.Should().Throw<ArgumentException>().WithMessage($"Invalid timezone: {invalidTimeZone}*").WithParameterName("timeZoneId");
    }

    [Fact]
    public void IsWithinExamWindow_WithCurrentTimeInWindow_ShouldReturnTrue()
    {
        // Arrange
        var currentUtc = DateTime.UtcNow;
        var exam = Exam.Create("Test", "Test", currentUtc.AddMinutes(-30), currentUtc.AddMinutes(30), 
            "UTC", 60, Guid.NewGuid(), 1);

        // Act
        var result = _timeZoneService.IsWithinExamWindow(currentUtc, exam.StartTime, exam.EndTime);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinExamWindow_WithCurrentTimeOutsideWindow_ShouldReturnFalse()
    {
        // Arrange
        var currentUtc = DateTime.UtcNow;
        var exam = Exam.Create("Test", "Test", currentUtc.AddHours(-2), currentUtc.AddHours(-1), 
            "UTC", 60, Guid.NewGuid(), 1);

        // Act
        var result = _timeZoneService.IsWithinExamWindow(currentUtc, exam.StartTime, exam.EndTime);

        // Assert
        result.Should().BeFalse();
    }
}