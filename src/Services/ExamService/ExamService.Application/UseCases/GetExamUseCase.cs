using Common.Application.Abstractions;
using ExamService.Application.DTOs;
using ExamService.Application.Queries;
using Common.Domain.Exceptions;
using ExamService.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace ExamService.Application.UseCases;

public class GetExamUseCase(
    IExamRepository examRepository,
    ICacheService cacheService,
    ILogger<GetExamUseCase> logger)
    : IQueryHandler<GetExamQuery, ExamDto>
{
    private readonly IExamRepository _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<GetExamUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ExamDto> HandleAsync(GetExamQuery query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"exam:{query.ExamId}";
        
        // Try cache first
        var cachedExam = await _cacheService.GetAsync<ExamDto>(cacheKey, cancellationToken);
        if (cachedExam != null)
        {
            return cachedExam;
        }

        // Get from database
        var exam = await _examRepository.GetByIdAsync(query.ExamId);
        if (exam == null)
        {
            throw new NotFoundException($"Exam not found: {query.ExamId}");
        }

        var examDto = MapToDto(exam);
        
        // Cache for 15 minutes
        await _cacheService.SetAsync(cacheKey, examDto, TimeSpan.FromMinutes(15), cancellationToken);

        return examDto;
    }

    private static ExamDto MapToDto(ExamService.Domain.Entities.Exam exam)
    {
        return new ExamDto
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            StartTime = exam.StartTime,
            EndTime = exam.EndTime,
            TimeZone = exam.TimeZone,
            DurationMinutes = exam.DurationMinutes,
            Status = exam.Status,
            CreatedByUserId = exam.CreatedByUserId,
            MaxAttempts = exam.MaxAttempts,
            IsPublished = exam.IsPublished,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt
        };
    }
}