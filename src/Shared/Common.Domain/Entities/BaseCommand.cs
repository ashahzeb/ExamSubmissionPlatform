using Common.Domain.Abstractions;

namespace Common.Domain.Entities;

public abstract class BaseCommand : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
}