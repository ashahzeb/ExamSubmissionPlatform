using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ExamService.Application.Abstractions;
using ExamService.Application.Commands;
using ExamService.Application.UseCases;
using ExamService.Domain.Entities;
using Common.Application.Abstractions;
using Common.Contracts.Events;
using Xunit;

namespace ExamService.Tests.Application;

public class CreateExamUseCaseTests
{
    private readonly Mock<IExamRepository> _mockRepository;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly Mock<ITimeZoneService> _mockTimeZoneService;
    private readonly Mock<ILogger<CreateExamUseCase>> _mockLogger;
    private readonly CreateExamUseCase _useCase;

    public CreateExamUseCaseTests()
    {
        _mockRepository = new Mock<IExamRepository>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        _mockTimeZoneService = new Mock<ITimeZoneService>();
        _mockLogger = new Mock<ILogger<CreateExamUseCase>>();

        _useCase = new CreateExamUseCase(
            _mockRepository.Object,
            _mockEventPublisher.Object,
            _mockTimeZoneService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateExam()
    {
        // Arrange
        var command = new CreateExamCommand
        {
            Title = "Math Exam",
            Description = "Final math examination",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            TimeZone = "UTC",
            DurationMinutes = 120,
            CreatedByUserId = Guid.NewGuid(),
            MaxAttempts = 3
        };

        var startTimeUtc = DateTime.UtcNow.AddDays(1);
        var endTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(2);

        _mockTimeZoneService.Setup(x => x.IsValidTimeZone(command.TimeZone))
            .Returns(true);
        _mockTimeZoneService.Setup(x => x.ConvertToUtc(command.StartTime, command.TimeZone))
            .Returns(startTimeUtc);
        _mockTimeZoneService.Setup(x => x.ConvertToUtc(command.EndTime, command.TimeZone))
            .Returns(endTimeUtc);

        // Act
        var result = await _useCase.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ExamId.Should().NotBe(Guid.Empty);
        result.Title.Should().Be(command.Title);
        result.Message.Should().Be("Exam created successfully");

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Exam>()), Times.Once);
        _mockEventPublisher.Verify(x => x.PublishAsync(It.IsAny<ExamCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidTimeZone_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateExamCommand
        {
            Title = "Math Exam",
            TimeZone = "Invalid/TimeZone"
        };

        _mockTimeZoneService.Setup(x => x.IsValidTimeZone(command.TimeZone))
            .Returns(false);

        // Act
        var result = await _useCase.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be($"Invalid timezone: {command.TimeZone}");

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Exam>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateExamCommand
        {
            Title = "Math Exam",
            TimeZone = "UTC"
        };

        _mockTimeZoneService.Setup(x => x.IsValidTimeZone(command.TimeZone))
            .Returns(true);
        _mockTimeZoneService.Setup(x => x.ConvertToUtc(It.IsAny<DateTime>(), command.TimeZone))
            .Returns(DateTime.UtcNow);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Exam>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _useCase.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("An error occurred while creating the exam");
    }
}