using ExamService.Application.Abstractions;
using ExamService.Domain.Entities;
using ExamService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExamService.Infrastructure.Repositories;

public class ExamRepository(ExamDbContext context, ILogger<ExamRepository> logger) : IExamRepository
{
    private readonly ExamDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<ExamRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Exam?> GetByIdAsync(Guid id)
    {
        return await _context.Exams
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Exam>> GetActiveExamsAsync(DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Exams
            .AsNoTracking()
            .Where(e => e.Status == ExamStatus.Active && e.IsPublished);

        if (startDate.HasValue)
        {
            query = query.Where(e => e.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.EndTime <= endDate.Value);
        }

        return await query
            .OrderBy(e => e.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetByCreatedByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Exams
            .AsNoTracking()
            .Where(e => e.CreatedByUserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Exams
            .AsNoTracking()
            .AnyAsync(e => e.Id == id);
    }

    public async Task AddAsync(Exam exam)
    {
        await _context.Exams.AddAsync(exam);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Exam created: {ExamId}", exam.Id);
    }

    public async Task UpdateAsync(Exam exam)
    {
        _context.Exams.Update(exam);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Exam updated: {ExamId}", exam.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam != null)
        {
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exam deleted: {ExamId}", id);
        }
    }
}