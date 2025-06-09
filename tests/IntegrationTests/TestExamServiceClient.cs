using System.Text.Json;
using SubmissionService.Application.Abstractions;
using SubmissionService.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

public class TestExamServiceClient : IExamServiceClient
{
    private readonly HttpClient _examClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public TestExamServiceClient(HttpClient examClient, IHttpContextAccessor httpContextAccessor)
    {
        _examClient = examClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ExamDetailsDto?> GetExamAsync(Guid examId)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _examClient.GetAsync($"/api/exam/{examId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var examDto = JsonSerializer.Deserialize<ExamService.Application.DTOs.ExamDto>(content, _options);
                
                return examDto != null ? new ExamDetailsDto
                {
                    Id = examDto.Id,
                    Title = examDto.Title,
                    StartTime = examDto.StartTime,
                    EndTime = examDto.EndTime,
                    DurationMinutes = examDto.DurationMinutes,
                    MaxAttempts = examDto.MaxAttempts,
                } : null;
            }
        }
        catch { }
        return null;
    }

    public async Task<bool> ExamExistsAsync(Guid examId)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _examClient.GetAsync($"/api/exam/{examId}/exists");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(content, _options);
            }
        }
        catch { }
        return false;
    }

    private void SetAuthorizationHeader()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            _examClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
        }
    }
}