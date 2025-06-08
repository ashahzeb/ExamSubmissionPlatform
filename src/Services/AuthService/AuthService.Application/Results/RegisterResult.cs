namespace AuthService.Application.Results;

public class RegisterResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}