namespace Common.Domain.Abstractions;

public interface ICommand
{
    Guid Id { get; }
}