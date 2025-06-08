using Common.Application.Abstractions;
using ExamService.Application.Abstractions;
using ExamService.Application.DTOs;
using ExamService.Application.Queries;
using Microsoft.Extensions.Logging;

namespace ExamService.Application.UseCases;

public class GetActiveExamsUseCase : IQueryHandler<GetActiveExamsQuery, IEnumerable<ExamDto>>
{
    private readonly IExamRepository _examRepository;
    private readonly ILogger<GetActiveExamsUseCase> _logger;

    public GetActiveExamsUseCase(IExamRepository examRepository, ILogger<GetActiveExamsUseCase> logger)
    {
        _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ExamDto>> HandleAsync(GetActiveExamsQuery query, CancellationToken cancellationToken = default)
    {
        var exams = await _examRepository.GetActiveExamsAsync(
            query.StartDate, query.EndDate, query.PageNumber, query.PageSize);

        return exams.Select(exam => new ExamDto
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
        });
    }
}