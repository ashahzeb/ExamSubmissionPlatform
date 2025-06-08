using Common.Domain.Abstractions;
using Common.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Infrastructure.Resilience;

public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisDistributedLockService> _logger;

    public RedisDistributedLockService(IConnectionMultiplexer redis, ILogger<RedisDistributedLockService> logger)
    {
        _database = redis?.GetDatabase() ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IDistributedLock> AcquireLockAsync(
        string lockKey, 
        TimeSpan expiry, 
        TimeSpan? timeout = null)
    {
        var lockId = Guid.NewGuid().ToString();
        var timeoutTime = DateTime.UtcNow.Add(timeout ?? TimeSpan.FromSeconds(30));

        while (DateTime.UtcNow < timeoutTime)
        {
            var acquired = await _database.StringSetAsync(
                lockKey, 
                lockId, 
                expiry, 
                When.NotExists);

            if (acquired)
            {
                _logger.LogDebug("Acquired distributed lock {LockKey} with ID {LockId}", lockKey, lockId);
                return new RedisDistributedLock(_database, lockKey, lockId, _logger);
            }

            await Task.Delay(100); // Wait before retry
        }

        throw new LockAcquisitionTimeoutException($"Failed to acquire lock {lockKey} within timeout period");
    }
}