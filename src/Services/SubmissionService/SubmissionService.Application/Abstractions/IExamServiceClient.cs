using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Abstractions;

public interface IExamServiceClient
{
    Task<ExamDetailsDto?> GetExamAsync(Guid examId);
    Task<bool> ExamExistsAsync(Guid examId);
    Task<bool> IsExamActiveAsync(Guid examId);
}