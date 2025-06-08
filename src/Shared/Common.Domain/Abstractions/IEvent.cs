namespace Common.Domain.Abstractions;

public interface IEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
}