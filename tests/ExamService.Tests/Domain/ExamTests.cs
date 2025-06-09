using FluentAssertions;
using ExamService.Domain.Entities;
using Xunit;

namespace ExamService.Tests.Domain;

public class ExamTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateExam()
    {
        // Arrange
        var title = "Math Exam";
        var description = "Final math examination";
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(2);
        var timeZone = "UTC";
        var durationMinutes = 120;
        var createdByUserId = Guid.NewGuid();
        var maxAttempts = 3;

        // Act
        var exam = Exam.Create(title, description, startTime, endTime, timeZone, durationMinutes, createdByUserId, maxAttempts);

        // Assert
        exam.Should().NotBeNull();
        exam.Title.Should().Be(title);
        exam.Description.Should().Be(description);
        exam.StartTime.Should().Be(startTime);
        exam.EndTime.Should().Be(endTime);
        exam.TimeZone.Should().Be(timeZone);
        exam.DurationMinutes.Should().Be(durationMinutes);
        exam.CreatedByUserId.Should().Be(createdByUserId);
        exam.MaxAttempts.Should().Be(maxAttempts);
        exam.Status.Should().Be(ExamStatus.Draft);
        exam.IsPublished.Should().BeFalse();
        exam.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string title)
    {
        // Act & Assert
        var act = () => Exam.Create(title, "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);
        act.Should().Throw<ArgumentException>().WithMessage("Title cannot be empty*");
    }

    [Fact]
    public void Create_WithEndTimeBeforeStartTime_ShouldThrowArgumentException()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = DateTime.UtcNow.AddDays(1).AddHours(-1); // Before start time

        // Act & Assert
        var act = () => Exam.Create("Title", "Description", startTime, endTime, "UTC", 120, Guid.NewGuid(), 3);
        act.Should().Throw<ArgumentException>().WithMessage("End time must be after start time");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void Create_WithInvalidDuration_ShouldThrowArgumentException(int durationMinutes)
    {
        // Act & Assert
        var act = () => Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", durationMinutes, Guid.NewGuid(), 3);
        act.Should().Throw<ArgumentException>().WithMessage("Duration must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Create_WithInvalidMaxAttempts_ShouldThrowArgumentException(int maxAttempts)
    {
        // Act & Assert
        var act = () => Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), maxAttempts);
        act.Should().Throw<ArgumentException>().WithMessage("Max attempts must be positive*");
    }

    [Fact]
    public void Publish_WithDraftExam_ShouldPublishExam()
    {
        // Arrange
        var exam = Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);

        // Act
        exam.Publish();

        // Assert
        exam.Status.Should().Be(ExamStatus.Active);
        exam.IsPublished.Should().BeTrue();
        exam.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Publish_WithCompletedExam_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var exam = Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);
        exam.Complete();

        // Act & Assert
        var act = () => exam.Publish();
        act.Should().Throw<InvalidOperationException>().WithMessage("Cannot publish a completed exam");
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted()
    {
        // Arrange
        var exam = Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);

        // Act
        exam.Complete();

        // Assert
        exam.Status.Should().Be(ExamStatus.Completed);
        exam.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var exam = Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);

        // Act
        exam.Cancel();

        // Assert
        exam.Status.Should().Be(ExamStatus.Cancelled);
        exam.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void IsActive_WithActiveAndPublishedExam_ShouldReturnTrue()
    {
        // Arrange
        var exam = Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);
        exam.Publish();

        // Act
        var isActive = exam.IsActive();

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WithDraftExam_ShouldReturnFalse()
    {
        // Arrange
        var exam = Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);

        // Act
        var isActive = exam.IsActive();

        // Assert
        isActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 1, false)] // Before start time
    [InlineData(0.5, 1, true)]  // Within time window
    [InlineData(2, 1, false)]   // After end time
    public void IsWithinTimeWindow_ShouldReturnCorrectResult(double hoursFromStart, double examDurationHours, bool expected)
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddHours(examDurationHours);
        var currentTime = startTime.AddHours(hoursFromStart);
        
        var exam = Exam.Create("Title", "Description", startTime, endTime, "UTC", 
            (int)(examDurationHours * 60), Guid.NewGuid(), 3);

        // Act
        var result = exam.IsWithinTimeWindow(currentTime);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void UpdateDetails_WithDraftExam_ShouldUpdateDetails()
    {
        // Arrange
        var exam = Exam.Create("Original Title", "Original Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);
        
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newStartTime = DateTime.UtcNow.AddDays(2);
        var newEndTime = DateTime.UtcNow.AddDays(2).AddHours(3);
        var newDuration = 180;
        var newMaxAttempts = 5;

        // Act
        exam.UpdateDetails(newTitle, newDescription, newStartTime, newEndTime, newDuration, newMaxAttempts);

        // Assert
        exam.Title.Should().Be(newTitle);
        exam.Description.Should().Be(newDescription);
        exam.StartTime.Should().Be(newStartTime);
        exam.EndTime.Should().Be(newEndTime);
        exam.DurationMinutes.Should().Be(newDuration);
        exam.MaxAttempts.Should().Be(newMaxAttempts);
        exam.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_WithActiveExam_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var exam = Exam.Create("Title", "Description", DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(1).AddHours(2), "UTC", 120, Guid.NewGuid(), 3);
        exam.Publish(); // Make it active

        // Act & Assert
        var act = () => exam.UpdateDetails("New Title", "New Description", DateTime.UtcNow.AddDays(2), 
            DateTime.UtcNow.AddDays(2).AddHours(3), 180, 5);
        act.Should().Throw<InvalidOperationException>().WithMessage("Cannot update active or completed exam");
    }
}