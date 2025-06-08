namespace AuthService.Application.Results;

public class TokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}