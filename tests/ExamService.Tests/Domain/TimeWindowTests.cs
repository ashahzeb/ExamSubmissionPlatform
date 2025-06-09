using FluentAssertions;
using ExamService.Domain.ValueObjects;
using Xunit;

namespace ExamService.Tests.Domain;

public class TimeWindowTests
{
    [Fact]
    public void Constructor_WithValidTimes_ShouldCreateTimeWindow()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddHours(2);
        var timeZone = "UTC";

        // Act
        var timeWindow = new TimeWindow(startTime, endTime, timeZone);

        // Assert
        timeWindow.StartTime.Should().Be(startTime);
        timeWindow.EndTime.Should().Be(endTime);
        timeZone.Should().Be(timeZone);
        timeWindow.Duration.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Constructor_WithEndTimeBeforeStartTime_ShouldThrowArgumentException()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddHours(-1);

        // Act & Assert
        var act = () => new TimeWindow(startTime, endTime, "UTC");
        act.Should().Throw<ArgumentException>().WithMessage("End time must be after start time");
    }

    [Theory]
    [InlineData(-1, false)] // Before start
    [InlineData(0, true)]   // At start
    [InlineData(1, true)]   // Within window
    [InlineData(2, true)]   // At end
    [InlineData(3, false)]  // After end
    public void Contains_ShouldReturnCorrectResult(int hoursOffset, bool expected)
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddHours(2);
        var timeWindow = new TimeWindow(startTime, endTime, "UTC");
        var testTime = startTime.AddHours(hoursOffset);

        // Act
        var result = timeWindow.Contains(testTime);

        // Assert
        result.Should().Be(expected);
    }
}