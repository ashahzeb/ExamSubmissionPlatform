namespace Common.Application.Abstractions;

public interface IQuery<TResult>
{
    Guid Id { get; }
}