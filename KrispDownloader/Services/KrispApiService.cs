using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using KrispDownloader.Configuration;
using KrispDownloader.Models;

namespace KrispDownloader.Services
{
    public class KrispApiService
    {
        private readonly HttpClient _httpClient;
        private readonly KrispApiConfiguration _configuration;
        private readonly ILogger<KrispApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public KrispApiService(HttpClient httpClient, IOptions<KrispApiConfiguration> configuration, ILogger<KrispApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration.Value;
            _logger = logger;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Set base URL and authorization header
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration.BearerToken}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "KrispDownloader/1.0");
        }

        public async Task<List<Meeting>> GetAllMeetingsAsync(CancellationToken cancellationToken = default)
        {
            var allMeetings = new List<Meeting>();
            int currentPage = 1;
            const int pageSize = 250;
            bool hasMorePages = true;

            _logger.LogInformation("Fetching all meetings");

            while (hasMorePages && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Fetching meetings page {Page}", currentPage);
                    
                    var request = new MeetingsListRequest
                    {
                        Sort = "desc",
                        SortKey = "created_at",
                        Page = currentPage,
                        Limit = pageSize,
                        Starred = false
                    };

                    var response = await GetMeetingsPageAsync(request, cancellationToken);
                    
                    if (response == null || response.Code != 0)
                    {
                        _logger.LogError("Failed to fetch meetings page {Page}. Response: {Response}", currentPage, response?.Message);
                        break;
                    }

                    var meetings = response.Data.Rows;
                    allMeetings.AddRange(meetings);
                    
                    _logger.LogDebug("Retrieved {Count} meetings from page {Page}. Total so far: {Total}", 
                        meetings.Count, currentPage, allMeetings.Count);

                    // Check if we have more pages
                    hasMorePages = meetings.Count == pageSize && allMeetings.Count < response.Data.Count;
                    currentPage++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching meetings page {Page}", currentPage);
                    break;
                }
            }

            _logger.LogInformation("Total meetings retrieved: {Count}", allMeetings.Count);
            return allMeetings;
        }

        private async Task<MeetingsListResponse?> GetMeetingsPageAsync(MeetingsListRequest request, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/v2/meetings/list", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP error {StatusCode} when fetching meetings: {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<MeetingsListResponse>(responseContent, _jsonOptions);
        }

        public async Task<MeetingDetailsResult?> GetMeetingDetailsAsync(string meetingId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Downloading meeting details for meeting {MeetingId}", meetingId);
                
                var response = await _httpClient.GetAsync($"/v2/meetings/{meetingId}", cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("HTTP error {StatusCode} when downloading transcript for meeting {MeetingId}: {ReasonPhrase}", 
                        response.StatusCode, meetingId, response.ReasonPhrase);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                MeetingDetailsResponse? parsed = null;
                try
                {
                    parsed = JsonSerializer.Deserialize<MeetingDetailsResponse>(content, _jsonOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize meeting details for meeting {MeetingId}", meetingId);
                }

                return new MeetingDetailsResult
                {
                    RawJson = content,
                    Parsed = parsed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading transcript for meeting {MeetingId}", meetingId);
                return null;
            }
        }

        public async Task<HttpResponseMessage?> GetRecordingResponseAsync(string recordingUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Downloading recording from {Url}", recordingUrl);
                
                var response = await _httpClient.GetAsync(recordingUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("HTTP error {StatusCode} when downloading recording: {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    response.Dispose();
                    return null;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading recording from {Url}", recordingUrl);
                return null;
            }
        }
    }
} 
