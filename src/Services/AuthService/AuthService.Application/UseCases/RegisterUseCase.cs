using AuthService.Application.Commands;
using AuthService.Application.Abstractions;
using AuthService.Application.Results;
using AuthService.Domain.Entities;
using Common.Application.Abstractions;
using Common.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.UseCases;

public class RegisterUseCase : ICommandHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RegisterUseCase> _logger;

    public RegisterUseCase(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IEventPublisher eventPublisher,
        ILogger<RegisterUseCase> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RegisterResult> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing registration for email: {Email}", command.Email);

        // Check if user already exists
        if (await _userRepository.ExistsAsync(command.Email))
        {
            return new RegisterResult
            {
                IsSuccess = false,
                Message = "User with this email already exists"
            };
        }

        // Validate password strength
        if (!_passwordService.IsPasswordStrong(command.Password))
        {
            return new RegisterResult
            {
                IsSuccess = false,
                Message = "Password does not meet security requirements"
            };
        }

        // Create user
        var passwordHash = _passwordService.HashPassword(command.Password);
        var user = User.Create(command.Email, command.FirstName, command.LastName, passwordHash);

        await _userRepository.AddAsync(user);

        // Publish event
        var userCreatedEvent = new UserCreatedEvent(user.Id, user.Email, $"{user.FirstName} {user.LastName}");
        await _eventPublisher.PublishAsync(userCreatedEvent, cancellationToken);

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        return new RegisterResult
        {
            UserId = user.Id,
            Email = user.Email,
            IsSuccess = true,
            Message = "User registered successfully"
        };
    }
}