using Common.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Resilience;

public abstract class ResilientDbContext(
    DbContextOptions options,
    IRetryService retryService,
    ILogger<ResilientDbContext> logger)
    : DbContext(options)
{
    private readonly IRetryService _retryService = retryService ?? throw new ArgumentNullException(nameof(retryService));
    private readonly ILogger<ResilientDbContext> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _retryService.ExecuteAsync(async () =>
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (SqlException ex) when (IsTransientError(ex))
            {
                _logger.LogWarning("Transient database error: {Error}", ex.Message);
                throw;
            }
        });
    }

    private static bool IsTransientError(SqlException ex)
    {
        // SQL Server transient error codes
        var transientErrorNumbers = new[]
        {
            2,      // Timeout
            53,     // Network path not found
            121,    // Semaphore timeout
            1205,   // Deadlock
            1222,   // Lock request timeout
            49918,  // Cannot process request
            49919,  // Cannot process create or update request
            49920   // Cannot process request
        };
        
        return transientErrorNumbers.Contains(ex.Number);
    }
}