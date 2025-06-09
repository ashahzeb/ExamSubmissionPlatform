using Common.Domain.Entities;

namespace ExamService.Domain.Entities;

public class Exam : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string TimeZone { get; private set; }
    public int DurationMinutes { get; private set; }
    public ExamStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public int MaxAttempts { get; private set; }
    public bool IsPublished { get; private set; }

    private Exam() { } // EF Constructor

    private Exam(string title, string description, DateTime startTime, 
        DateTime endTime, string timeZone, int durationMinutes, 
        Guid createdByUserId, int maxAttempts = 1)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? string.Empty;
        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        DurationMinutes = durationMinutes;
        CreatedByUserId = createdByUserId;
        MaxAttempts = maxAttempts;
        Status = ExamStatus.Active;
        IsPublished = false;
    }

    public static Exam Create(string title, string description, DateTime startTime,
        DateTime endTime, string timeZone, int durationMinutes, 
        Guid createdByUserId, int maxAttempts = 1)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");

        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationMinutes));

        if (maxAttempts <= 0)
            throw new ArgumentException("Max attempts must be positive", nameof(maxAttempts));

        return new Exam(title, description, startTime, endTime, timeZone, 
            durationMinutes, createdByUserId, maxAttempts);
    }

    public void Publish()
    {
        if (Status == ExamStatus.Completed)
            throw new InvalidOperationException("Cannot publish a completed exam");
        
        Status = ExamStatus.Active;
        IsPublished = true;
        UpdateTimestamp();
    }

    public void Complete()
    {
        Status = ExamStatus.Completed;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        Status = ExamStatus.Cancelled;
        UpdateTimestamp();
    }

    public bool IsActive() => Status == ExamStatus.Active && IsPublished;

    public bool IsWithinTimeWindow(DateTime currentTime)
    {
        return currentTime >= StartTime && currentTime <= EndTime;
    }

    public void UpdateDetails(string title, string description, DateTime startTime, 
        DateTime endTime, int durationMinutes, int maxAttempts)
    {
        if (Status == ExamStatus.Active || Status == ExamStatus.Completed)
            throw new InvalidOperationException("Cannot update active or completed exam");

        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? string.Empty;
        StartTime = startTime;
        EndTime = endTime;
        DurationMinutes = durationMinutes;
        MaxAttempts = maxAttempts;
        UpdateTimestamp();
    }
}

public enum ExamStatus
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}