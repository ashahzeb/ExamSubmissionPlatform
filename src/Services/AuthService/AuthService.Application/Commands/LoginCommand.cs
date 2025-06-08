using Common.Domain.Entities;

namespace AuthService.Application.Commands;

public class LoginCommand : BaseCommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}