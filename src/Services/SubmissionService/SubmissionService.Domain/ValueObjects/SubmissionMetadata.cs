namespace SubmissionService.Domain.ValueObjects;

public record SubmissionMetadata
{
    public string UserAgent { get; }
    public string IpAddress { get; }
    public DateTime StartedAt { get; }
    public DateTime SubmittedAt { get; }
    public TimeSpan Duration { get; }

    public SubmissionMetadata(string userAgent, string ipAddress, 
        DateTime startedAt, DateTime submittedAt)
    {
        UserAgent = userAgent ?? string.Empty;
        IpAddress = ipAddress ?? string.Empty;
        StartedAt = startedAt;
        SubmittedAt = submittedAt;
        Duration = submittedAt - startedAt;
    }
}