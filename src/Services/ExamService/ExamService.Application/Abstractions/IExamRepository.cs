using ExamService.Domain.Entities;

namespace ExamService.Application.Abstractions;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(Guid id);
    Task<IEnumerable<Exam>> GetActiveExamsAsync(DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<Exam>> GetByCreatedByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
    Task<bool> ExistsAsync(Guid id);
    Task AddAsync(Exam exam);
    Task UpdateAsync(Exam exam);
    Task DeleteAsync(Guid id);
}