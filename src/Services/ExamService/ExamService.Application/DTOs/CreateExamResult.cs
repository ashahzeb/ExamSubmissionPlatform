namespace ExamService.Application.DTOs;

public class CreateExamResult
{
    public Guid ExamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static CreateExamResult Success(Guid examId, string title, DateTime createdAt)
    {
        return new CreateExamResult
        {
            ExamId = examId,
            Title = title,
            CreatedAt = createdAt,
            IsSuccess = true,
            Message = "Exam created successfully"
        };
    }

    public static CreateExamResult Failure(string message)
    {
        return new CreateExamResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}