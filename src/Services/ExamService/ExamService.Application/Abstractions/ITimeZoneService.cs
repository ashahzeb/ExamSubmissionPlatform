using ExamService.Domain.Entities;

namespace ExamService.Application.Abstractions;

public interface ITimeZoneService
{
    DateTime ConvertToUtc(DateTime localDateTime, string timeZoneId);
    DateTime ConvertToExamTimeZone(DateTime utcDateTime, string timeZoneId);
    bool IsValidTimeZone(string timeZoneId);
    bool IsWithinExamWindow(DateTime currentUtc, Exam exam);
}