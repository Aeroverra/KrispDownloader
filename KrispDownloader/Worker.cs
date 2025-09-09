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
                _logger.LogInformation("JSON downloads directory: {Directory}", _fileService.GetDownloadDirectory());
                _logger.LogInformation("Formatted transcripts directory: {Directory}", _fileService.GetFormattedDirectory());

                // Get all meetings
                var meetings = await _krispApiService.GetAllMeetingsAsync(stoppingToken);
                
                if (meetings.Count == 0)
                {
                    _logger.LogInformation("No meetings found to download");
                    return;
                }

                // Filter meetings that have transcripts available
                var meetingsWithTranscripts = meetings.Where(m => 
                    m.Resources.Transcript.Status == "uploaded" || 
                    m.Resources.Transcript.Status == "ready").ToList();

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
                        _logger.LogInformation("Processing meeting: {Name} ({Id})", meeting.Name, meeting.Id);
                        
                        var transcript = await _krispApiService.GetTranscriptAsync(meeting.Id, stoppingToken);
                        
                        if (transcript != null)
                        {
                            // Save the original JSON
                            await _fileService.SaveTranscriptAsync(meeting, transcript);
                            
                            // Parse and save the formatted transcript
                            var formattedTranscript = _transcriptParsingService.ParseTranscriptToReadableFormat(transcript);
                            await _fileService.SaveFormattedTranscriptAsync(meeting, formattedTranscript);
                            
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
