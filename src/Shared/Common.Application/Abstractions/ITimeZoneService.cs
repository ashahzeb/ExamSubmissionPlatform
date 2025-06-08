namespace Common.Application.Abstractions;

public interface ITimeZoneService
{
    DateTime ConvertToUtc(DateTime localDateTime, string timeZoneId);
    DateTime ConvertToExamTimeZone(DateTime utcDateTime, string timeZoneId);
    bool IsValidTimeZone(string timeZoneId);
    bool IsWithinExamWindow(DateTime currentUtc, DateTime startTime, DateTime endTime);
}