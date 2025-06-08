namespace Common.Domain.Abstractions;

public interface IDistributedLockService
{
    Task<IDistributedLock> AcquireLockAsync(string lockKey, TimeSpan expiry, TimeSpan? timeout = null);
}