using System.Text.RegularExpressions;
using KrispDownloader.Models;

namespace KrispDownloader.Services
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly string _downloadDirectory;

        public FileService(ILogger<FileService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _downloadDirectory = configuration.GetValue<string>("DownloadDirectory") ?? "Downloads";
        }

        public async Task SaveTranscriptAsync(Meeting meeting, string transcriptContent)
        {
            try
            {
                // Ensure download directory exists
                Directory.CreateDirectory(_downloadDirectory);

                // Create a safe filename
                var fileName = CreateSafeFileName(meeting);
                var filePath = Path.Combine(_downloadDirectory, fileName);

                // Save the transcript
                await File.WriteAllTextAsync(filePath, transcriptContent);
                
                _logger.LogInformation("Saved transcript for meeting {MeetingId} to {FilePath}", meeting.Id, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transcript for meeting {MeetingId}", meeting.Id);
            }
        }

        private string CreateSafeFileName(Meeting meeting)
        {
            // Parse the date from created_at
            var createdDate = DateTime.TryParse(meeting.CreatedAt, out var date) ? date : DateTime.Now;
            var dateString = createdDate.ToString("yyyy-MM-dd_HH-mm-ss");
            
            // Clean the meeting name to make it file-safe
            var safeName = CleanFileName(meeting.Name);
            
            // Create filename with format: {date}_{safeName}_{meetingId}.json
            return $"{dateString}_{safeName}_{meeting.Id}.json";
        }

        private string CleanFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "unnamed_meeting";

            // Remove invalid characters and replace with underscores
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanName = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            
            // Remove extra spaces and replace with underscores
            cleanName = Regex.Replace(cleanName, @"\s+", "_");
            
            // Trim and limit length
            cleanName = cleanName.Trim('_').Substring(0, Math.Min(cleanName.Length, 100));
            
            return string.IsNullOrEmpty(cleanName) ? "unnamed_meeting" : cleanName;
        }

        public string GetDownloadDirectory()
        {
            return Path.GetFullPath(_downloadDirectory);
        }
    }
} 