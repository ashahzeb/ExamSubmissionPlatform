using System.Security.Claims;
using AuthService.Application.Results;
using AuthService.Domain.Entities;

namespace AuthService.Application.Abstractions;

public interface IJwtTokenService
{
    TokenResult GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}