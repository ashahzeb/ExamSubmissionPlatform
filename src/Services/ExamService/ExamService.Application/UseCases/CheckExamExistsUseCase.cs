using Common.Application.Abstractions;
using ExamService.Application.Abstractions;
using ExamService.Application.Queries;

namespace ExamService.Application.UseCases;

public class CheckExamExistsUseCase : IQueryHandler<CheckExamExistsQuery, bool>
{
    private readonly IExamRepository _examRepository;

    public CheckExamExistsUseCase(IExamRepository examRepository)
    {
        _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
    }

    public async Task<bool> HandleAsync(CheckExamExistsQuery query, CancellationToken cancellationToken = default)
    {
        return await _examRepository.ExistsAsync(query.ExamId);
    }
}