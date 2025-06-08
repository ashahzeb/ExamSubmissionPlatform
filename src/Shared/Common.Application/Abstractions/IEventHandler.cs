using Common.Domain.Abstractions;

namespace Common.Application.Abstractions;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}