using KrispDownloader.Services;

namespace KrispDownloader
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly KrispApiService _krispApiService;
        private readonly FileService _fileService;
        private readonly TranscriptParsingService _transcriptParsingService;

        public Worker(ILogger<Worker> logger, KrispApiService krispApiService, FileService fileService, TranscriptParsingService transcriptParsingService)
        {
            _logger = logger;
            _krispApiService = krispApiService;
            _fileService = fileService;
            _transcriptParsingService = transcriptParsingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting Krisp transcript download process...");

                // Get all meetings
                var meetings = await _krispApiService.GetAllMeetingsAsync(stoppingToken);
                
                if (meetings.Count == 0)
                {
                    _logger.LogInformation("No meetings found to download");
                    return;
                }

                // Filter meetings that have transcripts available
                var meetingsWithTranscripts = meetings
                    .Where(m => 
                        m.Resources.Transcript.Status == "uploaded" || 
                        m.Resources.Transcript.Status == "ready")
                    .ToList();

                _logger.LogInformation("Found {Total} meetings, {WithTranscripts} have transcripts available", 
                    meetings.Count, meetingsWithTranscripts.Count);

                // Download transcripts for each meeting
                int successCount = 0;
                int failureCount = 0;

                foreach (var meeting in meetingsWithTranscripts)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        _logger.LogInformation("Downloading meeting: {Name}", meeting.Name);
                        
                        var meetingDetailsResult = await _krispApiService.GetMeetingDetailsAsync(meeting.Id, stoppingToken);
                        
                        if (meetingDetailsResult != null && !string.IsNullOrWhiteSpace(meetingDetailsResult.RawJson))
                        {
                            // Save the original JSON
                            await _fileService.SaveMeetingDetailsJson(meeting, meetingDetailsResult.RawJson);
                            
                            // Parse and save the formatted transcript
                            var formattedTranscript = _transcriptParsingService.ParseTranscriptToReadableFormat(meetingDetailsResult.RawJson);
                            await _fileService.SaveFormattedTranscriptAsync(meeting, formattedTranscript);

                            // Download recording if available (prefer primary recording object)
                            var resources = meetingDetailsResult.Parsed?.Data?.Resources;
                            if (resources?.Recording != null)
                            {
                                var recordingCount = resources.Recordings?.Count ?? 0;
                                if (recordingCount > 1)
                                {
                                    _logger.LogCritical("Meeting {Id} has {Count} recordings; using the main recording URL", meeting.Id, recordingCount);
                                }

                                var recordingUrl = resources.Recording.Url;
                                var mimeType = resources.Recording.MimeType;

                                if (!string.IsNullOrWhiteSpace(recordingUrl))
                                {
                                    using var response = await _krispApiService.GetRecordingResponseAsync(recordingUrl, stoppingToken);
                                    if (response != null)
                                    {
                                        await using var stream = await response.Content.ReadAsStreamAsync(stoppingToken);
                                        await _fileService.SaveRecordingAsync(meeting, stream, mimeType, stoppingToken);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Failed to download recording for meeting {Id}", meeting.Id);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("Recording URL missing for meeting {Id}", meeting.Id);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Recording info missing for meeting {Id}", meeting.Id);
                            }
                            
                            successCount++;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to download transcript for meeting {Id}", meeting.Id);
                            failureCount++;
                        }

                        // Add a small delay to be respectful to the API
                        await Task.Delay(100, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing meeting {Id}", meeting.Id);
                        failureCount++;
                    }
                }

                _logger.LogInformation("Download process completed. Success: {Success}, Failures: {Failures}", 
                    successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error during transcript download process");
            }
        }
    }
}
