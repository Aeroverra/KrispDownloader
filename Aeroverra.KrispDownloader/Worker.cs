using System.Text.Json;
using Aeroverra.KrispDownloader.Configuration;
using Aeroverra.KrispDownloader.Models;
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

                var meetings = await _krispApiService.GetAllMeetingsAsync(stoppingToken);

                if (meetings.Count == 0)
                {
                    _logger.LogInformation("No meetings found to download");
                    return;
                }

                var meetingsWithTranscripts = meetings
                    .Where(m =>
                        m.Resources.Transcript.Status == "uploaded" ||
                        m.Resources.Transcript.Status == "ready")
                    .ToList();

                _logger.LogInformation("Found {Total} meetings, {WithTranscripts} have transcripts available",
                    meetings.Count, meetingsWithTranscripts.Count);

                int successCount = 0;
                int failureCount = 0;

                foreach (var meeting in meetingsWithTranscripts)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        _logger.LogInformation("Downloading meeting: {Name}", meeting.Name);

                        var meetingDetailsResult = await _krispApiService.GetMeetingDetailsAsync(meeting.Id, stoppingToken);

                        if (meetingDetailsResult != null && !string.IsNullOrWhiteSpace(meetingDetailsResult.RawJson))
                        {
                            if (_configuration.SaveMeetingDetails)
                            {
                                await _fileService.SaveMeetingDetailsJson(meeting, meetingDetailsResult.RawJson);
                            }

                            if (_configuration.SaveTranscripts)
                            {
                                var formattedTranscript = _transcriptParsingService.ParseTranscriptToReadableFormat(meetingDetailsResult.RawJson);
                                await _fileService.SaveFormattedTranscriptAsync(meeting, formattedTranscript);
                            }

                            if (_configuration.SaveRecordings)
                            {
                                var recordingDetails = ExtractRecordingsFromBlockTree(meetingDetailsResult.Parsed);

                                if (recordingDetails.Count == 0)
                                {
                                    _logger.LogWarning("Recording info missing for meeting {Id}", meeting.Id);
                                }

                                for (int i = 0; i < recordingDetails.Count; i++)
                                {
                                    var recording = recordingDetails[i];
                                    var label = string.IsNullOrWhiteSpace(recording.CaptureType)
                                        ? $"recording_{i + 1}"
                                        : recording.CaptureType;

                                    if (string.IsNullOrWhiteSpace(recording.Url))
                                    {
                                        _logger.LogWarning("Recording URL missing for meeting {Id} ({Label})", meeting.Id, label);
                                        continue;
                                    }

                                    _logger.LogDebug("Downloading recording {Index}/{Total} ({Label}) for meeting {Name}",
                                        i + 1, recordingDetails.Count, label, meeting.Name);

                                    using var response = await _krispApiService.GetRecordingResponseAsync(recording.Url, stoppingToken);
                                    if (response != null)
                                    {
                                        await using var stream = await response.Content.ReadAsStreamAsync(stoppingToken);
                                        await _fileService.SaveRecordingAsync(meeting, stream, recording, i, stoppingToken);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Failed to download recording {Index} for meeting {Id}", i + 1, meeting.Id);
                                    }
                                }
                            }

                            successCount++;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to download transcript for meeting {Id}", meeting.Id);
                            failureCount++;
                        }

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

        private List<RecordingDetail> ExtractRecordingsFromBlockTree(Block? rootBlock)
        {
            var recordings = new List<RecordingDetail>();
            if (rootBlock == null)
            {
                return recordings;
            }

            var recordingBlocks = FindAllBlocksByType(rootBlock, "recording");
            foreach (var block in recordingBlocks)
            {
                if (block.Content == null || block.Content.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var content = block.Content.Value;
                var detail = new RecordingDetail
                {
                    Id = block.Id,
                    Url = content.TryGetProperty("url", out var url) ? url.GetString() : null,
                    MimeType = content.TryGetProperty("mime_type", out var mime) ? mime.GetString() : null,
                    CaptureType = content.TryGetProperty("capture_type", out var capture) ? capture.GetString() : null,
                    Size = content.TryGetProperty("size", out var size) && size.ValueKind == JsonValueKind.Number ? size.GetInt64() : null,
                    Status = content.TryGetProperty("status", out var status) ? status.GetString() : null,
                    CreatedAt = content.TryGetProperty("created_at", out var created) ? created.GetString() : null
                };

                recordings.Add(detail);
            }

            return recordings;
        }

        private List<Block> FindAllBlocksByType(Block block, string blockType)
        {
            var results = new List<Block>();

            if (block.BlockType == blockType)
            {
                results.Add(block);
            }

            foreach (var child in block.Children)
            {
                results.AddRange(FindAllBlocksByType(child, blockType));
            }

            return results;
        }
    }
}
