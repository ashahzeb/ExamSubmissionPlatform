using Common.Domain;
using Common.Domain.Entities;

namespace SubmissionService.Domain.Entities;

public class ExamSubmission : BaseEntity
{
    public Guid ExamId { get; private set; }
    public Guid UserId { get; private set; }
    public string Content { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public string ContentHash { get; private set; }
    public int AttemptNumber { get; private set; }
    public TimeSpan TimeTaken { get; private set; }
    public string? FailureReason { get; private set; }
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }

    private ExamSubmission() { } // EF Constructor

    private ExamSubmission(Guid examId, Guid userId, string content, 
        DateTime submittedAt, int attemptNumber, TimeSpan timeTaken,
        string? userAgent = null, string? ipAddress = null)
    {
        ExamId = examId;
        UserId = userId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        SubmittedAt = submittedAt;
        Status = SubmissionStatus.Submitted;
        ContentHash = ComputeHash(content);
        AttemptNumber = attemptNumber;
        TimeTaken = timeTaken;
        UserAgent = userAgent;
        IpAddress = ipAddress;
    }

    public static ExamSubmission Create(Guid examId, Guid userId, string content, 
        DateTime submittedAt, int attemptNumber = 1, TimeSpan? timeTaken = null,
        string? userAgent = null, string? ipAddress = null)
    {
        if (examId == Guid.Empty)
            throw new ArgumentException("ExamId cannot be empty", nameof(examId));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        if (attemptNumber <= 0)
            throw new ArgumentException("Attempt number must be positive", nameof(attemptNumber));

        return new ExamSubmission(examId, userId, content, submittedAt, 
            attemptNumber, timeTaken ?? TimeSpan.Zero, userAgent, ipAddress);
    }

    public void MarkAsProcessed()
    {
        Status = SubmissionStatus.Processed;
        UpdateTimestamp();
    }

    public void MarkAsFailed(string reason)
    {
        Status = SubmissionStatus.Failed;
        FailureReason = reason;
        UpdateTimestamp();
    }

    public void MarkAsGraded(decimal? score = null, string? feedback = null)
    {
        Status = SubmissionStatus.Graded;
        // Additional grading properties would go here
        UpdateTimestamp();
    }

    public bool IsValid()
    {
        return ContentHash == ComputeHash(Content);
    }

    public bool IsLatestAttempt(int totalAttempts)
    {
        return AttemptNumber == totalAttempts;
    }

    private string ComputeHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashBytes);
    }
}

public enum SubmissionStatus
{
    Submitted = 0,
    Processed = 1,
    Graded = 2,
    Failed = 3
}