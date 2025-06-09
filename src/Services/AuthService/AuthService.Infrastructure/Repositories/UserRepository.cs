using AuthService.Application.Abstractions;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AuthDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        try
        {
            _logger.LogInformation("Adding user to context: {UserId}, {Email}", user.Id, user.Email);
            
            // Add to context
            var entityEntry = await _context.Users.AddAsync(user);
            _logger.LogInformation("User added to context with state: {State}", entityEntry.State);
            
            // Save changes
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("SaveChanges result: {Result} rows affected", result);
            
            if (result == 0)
            {
                throw new InvalidOperationException($"Failed to save user {user.Id} - no rows affected");
            }
            
            _logger.LogInformation("User created successfully: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user: {UserId}, {Email}", user.Id, user.Email);
            throw;
        }
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User updated: {UserId}", user.Id);
    }
}