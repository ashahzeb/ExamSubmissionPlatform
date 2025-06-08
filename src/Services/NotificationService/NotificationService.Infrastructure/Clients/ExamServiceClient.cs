using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;

namespace NotificationService.Infrastructure.Clients;

public class ExamServiceClient(HttpClient httpClient, ILogger<ExamServiceClient> logger) : IExamServiceClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ExamServiceClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ExamDto?> GetExamAsync(Guid examId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/exam/{examId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExamDto>(content, _jsonOptions);
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            _logger.LogWarning("Failed to get exam {ExamId}. Status: {StatusCode}", 
                examId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam {ExamId}", examId);
            // Return a default exam to prevent notification failures
            return new ExamDto
            {
                Id = examId,
                Title = "Unknown Exam"
            };
        }
    }
}