using Common.Domain.Abstractions;

namespace Common.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent;
}