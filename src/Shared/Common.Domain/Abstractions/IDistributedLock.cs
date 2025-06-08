namespace Common.Domain.Abstractions;

public interface IDistributedLock : IDisposable
{
    Task ReleaseLockAsync();
}