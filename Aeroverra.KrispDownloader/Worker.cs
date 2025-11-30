using Aeroverra.KrispDownloader.Configuration;
using Aeroverra.KrispDownloader.Services;

namespace Aeroverra.KrispDownloader
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly KrispApiService _krispApiService;
        private readonly FileService _fileService;
        private readonly TranscriptParsingService _transcriptParsingService;
        private readonly KrispApiConfiguration _configuration;

        public Worker(
            ILogger<Worker> logger,
            KrispApiService krispApiService,
            FileService fileService,
            TranscriptParsingService transcriptParsingService,
            Microsoft.Extensions.Options.IOptions<KrispApiConfiguration> configuration)
        {
            _logger = logger;
            _krispApiService = krispApiService;
            _fileService = fileService;
            _transcriptParsingService = transcriptParsingService;
            _configuration = configuration.Value;
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
                            if (_configuration.SaveMeetingDetails)
                            {
                                await _fileService.SaveMeetingDetailsJson(meeting, meetingDetailsResult.RawJson);
                            }

                            if (_configuration.SaveTranscripts)
                            {
                                // Parse and save the formatted transcript
                                var formattedTranscript = _transcriptParsingService.ParseTranscriptToReadableFormat(meetingDetailsResult.RawJson);
                                await _fileService.SaveFormattedTranscriptAsync(meeting, formattedTranscript);
                            }

                            if (_configuration.SaveRecordings)
                            {
                                // Download recording if available
                                var resources = meetingDetailsResult.Parsed?.Data?.Resources;
                                if (resources?.Recording != null)
                                {
                                    var recordingCount = resources.Recordings?.Count ?? 0;
                                    if (recordingCount > 1)
                                    {
                                        _logger.LogCritical("Meeting {Id} has {Count} recordings. Support for this may need to be added", meeting.Id, recordingCount);
                                        Console.Title = $"{Program.ConsoleTitle} CRITICAL: Meeting {meeting.Id} has {recordingCount} recordings!";
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
