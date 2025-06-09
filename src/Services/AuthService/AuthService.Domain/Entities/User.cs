using Common.Domain.Entities;

namespace AuthService.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; protected set; } = string.Empty;
    public string FirstName { get; protected set; } = string.Empty;
    public string LastName { get; protected set; } = string.Empty;
    public string PasswordHash { get; protected set; } = string.Empty;
    public bool IsActive { get; protected set; } = true;
    public DateTime? LastLoginAt { get; protected set; }

    // Parameterless constructor for EF Core
    public User() { }

    private User(string email, string firstName, string lastName, string passwordHash)
    {
        Email = email?.Trim() ?? string.Empty;
        FirstName = firstName?.Trim() ?? string.Empty;
        LastName = lastName?.Trim() ?? string.Empty;
        PasswordHash = passwordHash ?? string.Empty;
        IsActive = true;
    }

    public static User Create(string email, string firstName, string lastName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        return new User(email, firstName, lastName, passwordHash);
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}