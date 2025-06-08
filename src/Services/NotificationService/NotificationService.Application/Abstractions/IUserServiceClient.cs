using NotificationService.Application.DTOs;

namespace NotificationService.Application.Abstractions;

public interface IUserServiceClient
{
    Task<UserDto?> GetUserAsync(Guid userId);
}