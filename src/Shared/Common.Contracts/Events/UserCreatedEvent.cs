using Common.Domain.Entities;

namespace Common.Contracts.Events;

public class UserCreatedEvent(Guid userId, string email, string fullName) : BaseEvent
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
    public string FullName { get; } = fullName;
}