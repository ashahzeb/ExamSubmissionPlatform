using SubmissionService.Domain.Entities;

namespace SubmissionService.Application.Abstractions;

public interface ISubmissionRepository
{
    Task<ExamSubmission?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExamSubmission>> GetByUserAndExamAsync(Guid userId, Guid examId);
    Task<IEnumerable<ExamSubmission>> GetByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<ExamSubmission>> GetByExamAsync(Guid examId, int pageNumber = 1, int pageSize = 20);
    Task<bool> ExistsAsync(Guid userId, Guid examId);
    Task<int> GetAttemptCountAsync(Guid userId, Guid examId);
    Task AddAsync(ExamSubmission submission);
    Task UpdateAsync(ExamSubmission submission);
}