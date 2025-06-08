using NotificationService.Application.DTOs;

namespace NotificationService.Application.Abstractions;

public interface IExamServiceClient
{
    Task<ExamDto?> GetExamAsync(Guid examId);
}