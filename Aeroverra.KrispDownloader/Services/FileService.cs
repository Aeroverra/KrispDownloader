using System.Text.RegularExpressions;
using Aeroverra.KrispDownloader.Models;
using Aeroverra.KrispDownloader.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Aeroverra.KrispDownloader.Services
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly string _meetingDetailsDirectory;
        private readonly string _formattedDirectory;
        private readonly string _recordingsDirectory;

        public FileService(ILogger<FileService> logger, IOptions<KrispApiConfiguration> configuration)
        {
            _logger = logger;
            var config = configuration.Value;
            _meetingDetailsDirectory = config.MeetingDetailsOutput ?? "Downloads";
            _formattedDirectory = config.TranscriptsOutput ?? "Downloads";
            _recordingsDirectory = config.RecordingsOutput ?? "Downloads";
        }

        public async Task SaveMeetingDetailsJson(Meeting meeting, string transcriptContent)
        {
            try
            {
                // Ensure download directory exists
                Directory.CreateDirectory(_meetingDetailsDirectory);

                // Format JSON for readability; fall back to original on parse errors
                string formattedJson;
                try
                {
                    using var doc = JsonDocument.Parse(transcriptContent);
                    formattedJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                }
                catch (JsonException e)
                {
                    formattedJson = transcriptContent;
                    _logger.LogError(e, "Failed to parse JSON for meeting {MeetingId}, saving unformatted content", meeting.Id);
                }

                // Create a safe filename
                var fileName = CreateSafeFileName(meeting, ".json");
                var filePath = Path.Combine(_meetingDetailsDirectory, fileName);

                // Save the JSON transcript
                await File.WriteAllTextAsync(filePath, formattedJson);
                
                _logger.LogDebug("Saved JSON transcript for meeting {MeetingId} to {FilePath}", meeting.Id, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving JSON transcript for meeting {MeetingId}", meeting.Id);
            }
        }

        public async Task SaveFormattedTranscriptAsync(Meeting meeting, string formattedContent)
        {
            try
            {
                // Ensure formatted directory exists
                Directory.CreateDirectory(_formattedDirectory);

                // Create a safe filename
                var fileName = CreateSafeFileName(meeting, ".txt");
                var filePath = Path.Combine(_formattedDirectory, fileName);

                // Save the formatted transcript
                await File.WriteAllTextAsync(filePath, formattedContent);
                
                _logger.LogDebug("Saved formatted transcript for meeting {MeetingId} to {FilePath}", meeting.Id, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving formatted transcript for meeting {MeetingId}", meeting.Id);
            }
        }

        private string CreateSafeFileName(Meeting meeting, string extension, string? extraSuffix = null)
        {
            // Parse the date from created_at
            var createdDate = DateTime.TryParse(meeting.CreatedAt, out var date) ? date : DateTime.Now;
            var dateString = createdDate.ToString("yyyy-MM-dd_HH-mm-ss");
            
            // Clean the meeting name to make it file-safe
            var safeName = CleanPathSegment(meeting.Name);
            var suffix = string.IsNullOrWhiteSpace(extraSuffix) ? string.Empty : $"_{CleanPathSegment(extraSuffix)}";
            
            // Create filename with format: {date}_{safeName}_{meetingId}.{extension}
            return $"{dateString}_{safeName}_{meeting.Id}{suffix}{extension}";
        }

        private string CleanPathSegment(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "unnamed_meeting";

            // Remove invalid characters and replace with underscores
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanName = new string(value.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            
            // Remove extra spaces and replace with underscores
            cleanName = Regex.Replace(cleanName, @"\s+", "_");
            
            // Trim and limit length
            cleanName = cleanName.Trim('_').Substring(0, Math.Min(cleanName.Length, 100));
            
            return string.IsNullOrEmpty(cleanName) ? "unnamed_meeting" : cleanName;
        }

        public string GetDownloadDirectory()
        {
            return Path.GetFullPath(_meetingDetailsDirectory);
        }

        public string GetFormattedDirectory()
        {
            return Path.GetFullPath(_formattedDirectory);
        }

        public string GetRecordingsDirectory()
        {
            return Path.GetFullPath(_recordingsDirectory);
        }

        public async Task SaveRecordingAsync(
            Meeting meeting,
            Stream contentStream,
            RecordingDetail recording,
            int recordingIndex,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Directory.CreateDirectory(_recordingsDirectory);

                var extension = GetExtensionFromMimeType(recording.MimeType);

                var suffixParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(recording.CaptureType))
                {
                    suffixParts.Add(recording.CaptureType);
                }

                if (!string.IsNullOrWhiteSpace(recording.Id))
                {
                    suffixParts.Add(recording.Id!);
                }
                else if (recordingIndex >= 0)
                {
                    suffixParts.Add($"part{recordingIndex + 1:D2}");
                }

                var suffix = suffixParts.Count > 0
                    ? string.Join("_", suffixParts)
                    : $"part{recordingIndex + 1:D2}";

                var fileName = CreateSafeFileName(meeting, extension, suffix);
                var filePath = Path.Combine(_recordingsDirectory, fileName);

                await using var fileStream = File.Create(filePath);
                await contentStream.CopyToAsync(fileStream, cancellationToken);

                _logger.LogDebug("Saved recording for meeting {MeetingId} to {FilePath}", meeting.Id, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving recording for meeting {MeetingId}", meeting.Id);
            }
        }

        private string GetExtensionFromMimeType(string? mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
                return ".bin";

            return mimeType.ToLowerInvariant() switch
            {
                "audio/mp3" => ".mp3",
                "audio/mpeg" => ".mp3",
                "audio/wav" => ".wav",
                "audio/x-wav" => ".wav",
                "audio/flac" => ".flac",
                "video/mp4" => ".mp4",
                "video/webm" => ".webm",
                "video/mpeg" => ".mpeg",
                _ => ".bin"
            };
        }
    }
}
