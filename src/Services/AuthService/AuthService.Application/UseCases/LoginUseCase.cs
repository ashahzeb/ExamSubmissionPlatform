using AuthService.Application.Commands;
using AuthService.Application.Abstractions;
using AuthService.Application.Results;
using Common.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.UseCases;

public class LoginUseCase : ICommandHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginUseCase> _logger;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        ILogger<LoginUseCase> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing login for email: {Email}", command.Email);

        var user = await _userRepository.GetByEmailAsync(command.Email);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Login failed - user not found or inactive: {Email}", command.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var isPasswordValid = _passwordService.VerifyPassword(command.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed - invalid password: {Email}", command.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);

        var token = _jwtTokenService.GenerateToken(user);

        _logger.LogInformation("Login successful for user: {UserId}", user.Id);

        return new LoginResult
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            UserId = user.Id,
            Email = user.Email
        };
    }
}