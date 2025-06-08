namespace Common.Contracts.DTOs;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}