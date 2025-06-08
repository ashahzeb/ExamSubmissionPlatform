using Common.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Infrastructure.Resilience;

public class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _lockKey;
    private readonly string _lockId;
    private readonly ILogger _logger;
    private bool _disposed;

    internal RedisDistributedLock(IDatabase database, string lockKey, string lockId, ILogger logger)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _lockKey = lockKey ?? throw new ArgumentNullException(nameof(lockKey));
        _lockId = lockId ?? throw new ArgumentNullException(nameof(lockId));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReleaseLockAsync()
    {
        if (_disposed) return;

        const string script = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";

        var result = await _database.ScriptEvaluateAsync(script, new RedisKey[] { _lockKey }, new RedisValue[] { _lockId });

        if (result.Equals(1))
        {
            _logger.LogDebug("Released distributed lock {LockKey} with ID {LockId}", _lockKey, _lockId);
        }
        else
        {
            _logger.LogWarning("Failed to release distributed lock {LockKey} - lock may have expired", _lockKey);
        }

        _disposed = true;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Task.Run(async () => await ReleaseLockAsync());
        }
    }
}