using System.Net.Http.Headers;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Abstractions;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SubmissionService.Infrastructure.Clients;

public class ExamServiceClient : IExamServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExamServiceClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExamServiceClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ExamServiceClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        SetAuthorizationHeader();
    }
    
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
    
    
    private void SetAuthorizationHeader()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
        }
    }
}