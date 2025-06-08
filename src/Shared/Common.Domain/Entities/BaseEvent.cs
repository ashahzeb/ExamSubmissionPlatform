using Common.Domain.Abstractions;

namespace Common.Domain.Entities;

public abstract class BaseEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}