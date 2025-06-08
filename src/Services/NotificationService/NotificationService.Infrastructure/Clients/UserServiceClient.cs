using NotificationService.Application.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;

namespace NotificationService.Infrastructure.Clients;

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            _logger.LogWarning("Failed to get user {UserId}. Status: {StatusCode}", 
                userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            // Return a default user to prevent notification failures
            return new UserDto
            {
                Id = userId,
                Email = "unknown@example.com",
                FullName = "Unknown User"
            };
        }
    }
}