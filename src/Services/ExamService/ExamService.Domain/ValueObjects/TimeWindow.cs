namespace ExamService.Domain.ValueObjects;

public record TimeWindow
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string TimeZone { get; }

    public TimeWindow(DateTime startTime, DateTime endTime, string timeZone)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");
        
        StartTime = startTime;
        EndTime = endTime;
        TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }

    public bool Contains(DateTime dateTime)
    {
        return dateTime >= StartTime && dateTime <= EndTime;
    }

    public TimeSpan Duration => EndTime - StartTime;
}