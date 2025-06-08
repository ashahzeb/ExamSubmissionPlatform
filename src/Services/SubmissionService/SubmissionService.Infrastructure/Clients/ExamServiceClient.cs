using SubmissionService.Application.DTOs;
using SubmissionService.Application.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SubmissionService.Infrastructure.Clients;

public class ExamServiceClient(HttpClient httpClient, ILogger<ExamServiceClient> logger) : IExamServiceClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ExamServiceClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ExamDetailsDto?> GetExamAsync(Guid examId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/exam/{examId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExamDetailsDto>(content, _jsonOptions);
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
            throw;
        }
    }

    public async Task<bool> ExamExistsAsync(Guid examId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/exam/{examId}/exists");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(content, _jsonOptions);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if exam exists {ExamId}", examId);
            return false;
        }
    }

    public async Task<bool> IsExamActiveAsync(Guid examId)
    {
        var exam = await GetExamAsync(examId);
        return exam?.IsActive ?? false;
    }
}