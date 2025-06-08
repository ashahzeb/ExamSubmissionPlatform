using SubmissionService.Application.Abstractions;
using SubmissionService.Domain.Entities;
using SubmissionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SubmissionService.Infrastructure.Repositories;

public class SubmissionRepository(SubmissionDbContext context, ILogger<SubmissionRepository> logger)
    : ISubmissionRepository
{
    private readonly SubmissionDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<SubmissionRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ExamSubmission?> GetByIdAsync(Guid id)
    {
        return await _context.Submissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<ExamSubmission>> GetByUserAndExamAsync(Guid userId, Guid examId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.ExamId == examId)
            .OrderBy(s => s.AttemptNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExamSubmission>> GetByUserAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExamSubmission>> GetByExamAsync(Guid examId, int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Where(s => s.ExamId == examId)
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid examId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .AnyAsync(s => s.UserId == userId && s.ExamId == examId);
    }

    public async Task<int> GetAttemptCountAsync(Guid userId, Guid examId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .CountAsync(s => s.UserId == userId && s.ExamId == examId);
    }

    public async Task AddAsync(ExamSubmission submission)
    {
        await _context.Submissions.AddAsync(submission);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Submission created: {SubmissionId}", submission.Id);
    }

    public async Task UpdateAsync(ExamSubmission submission)
    {
        _context.Submissions.Update(submission);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Submission updated: {SubmissionId}", submission.Id);
    }
}