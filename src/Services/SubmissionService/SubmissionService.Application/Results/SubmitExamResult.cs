namespace SubmissionService.Application.Results;

public class SubmitExamResult
{
    public Guid SubmissionId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int AttemptNumber { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public static SubmitExamResult Success(Guid submissionId, DateTime submittedAt, int attemptNumber)
    {
        return new SubmitExamResult
        {
            SubmissionId = submissionId,
            SubmittedAt = submittedAt,
            AttemptNumber = attemptNumber,
            IsSuccess = true,
            Message = "Submission successful"
        };
    }

    public static SubmitExamResult Failure(string message)
    {
        return new SubmitExamResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}