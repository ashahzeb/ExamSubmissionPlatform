using ExamService.Application.Abstractions;
using ExamService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ExamService.Infrastructure.Services;

public class TimeZoneService(ILogger<TimeZoneService> logger) : ITimeZoneService
{
    private readonly ILogger<TimeZoneService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public DateTime ConvertToUtc(DateTime localDateTime, string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting time to UTC for timezone {TimeZoneId}", timeZoneId);
            throw new ArgumentException($"Invalid timezone: {timeZoneId}", nameof(timeZoneId));
        }
    }

    public DateTime ConvertToExamTimeZone(DateTime utcDateTime, string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting UTC time to timezone {TimeZoneId}", timeZoneId);
            throw new ArgumentException($"Invalid timezone: {timeZoneId}", nameof(timeZoneId));
        }
    }

    public bool IsValidTimeZone(string timeZoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsWithinExamWindow(DateTime currentUtc, Exam exam)
    {
        try
        {
            var examStartUtc = exam.StartTime;
            var examEndUtc = exam.EndTime;

            return currentUtc >= examStartUtc && currentUtc <= examEndUtc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking exam window for exam {ExamId}", exam.Id);
            return false;
        }
    }
}