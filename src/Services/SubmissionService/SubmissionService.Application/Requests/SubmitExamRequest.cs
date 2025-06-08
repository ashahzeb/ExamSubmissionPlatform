namespace SubmissionService.Application.Requests;

public class SubmitExamRequest
{
    public Guid ExamId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
}