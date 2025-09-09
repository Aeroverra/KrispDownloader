using System.Text.Json;
using KrispDownloader.Models;

namespace KrispDownloader.Services
{
    public class TranscriptParsingService
    {
        private readonly ILogger<TranscriptParsingService> _logger;

        public TranscriptParsingService(ILogger<TranscriptParsingService> logger)
        {
            _logger = logger;
        }

        public string ParseTranscriptToReadableFormat(string jsonContent)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;

                // Extract speaker information
                var speakerMap = ExtractSpeakerMap(root);

                // Extract transcript content
                var transcriptContent = ExtractTranscriptContent(root);

                // Format the transcript
                return FormatTranscript(transcriptContent, speakerMap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing transcript JSON");
                return "Error parsing transcript content";
            }
        }

        private Dictionary<int, string> ExtractSpeakerMap(JsonElement root)
        {
            var speakerMap = new Dictionary<int, string>();

            if (root.TryGetProperty("data", out var data) && 
                data.TryGetProperty("speakers", out var speakers) && 
                speakers.TryGetProperty("data", out var speakerData))
            {
                foreach (var speakerProperty in speakerData.EnumerateObject())
                {
                    if (int.TryParse(speakerProperty.Name, out var speakerIndex))
                    {
                        var speaker = speakerProperty.Value;
                        if (speaker.TryGetProperty("person", out var person))
                        {
                            var firstName = person.TryGetProperty("first_name", out var fn) ? fn.GetString() : "";
                            var lastName = person.TryGetProperty("last_name", out var ln) ? ln.GetString() : "";
                            
                            var displayName = !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName)
                                ? $"{firstName} {lastName}"
                                : !string.IsNullOrEmpty(firstName) ? firstName
                                : !string.IsNullOrEmpty(lastName) ? lastName
                                : $"Speaker {speakerIndex}";

                            speakerMap[speakerIndex] = displayName;
                        }
                    }
                }
            }

            return speakerMap;
        }

        private List<TranscriptEntry> ExtractTranscriptContent(JsonElement root)
        {
            var entries = new List<TranscriptEntry>();

            if (root.TryGetProperty("data", out var data) && 
                data.TryGetProperty("resources", out var resources) && 
                resources.TryGetProperty("transcript", out var transcript) && 
                transcript.TryGetProperty("content", out var contentProperty))
            {
                var contentString = contentProperty.GetString();
                if (!string.IsNullOrEmpty(contentString))
                {
                    var transcriptArray = JsonSerializer.Deserialize<JsonElement[]>(contentString);
                    if (transcriptArray != null)
                    {
                        foreach (var item in transcriptArray)
                        {
                            if (item.TryGetProperty("speakerIndex", out var speakerIndexProp) &&
                                item.TryGetProperty("speech", out var speech))
                            {
                                var speakerIndex = speakerIndexProp.GetInt32();
                                var startTime = speech.TryGetProperty("start", out var start) ? start.GetDouble() : 0;
                                var text = speech.TryGetProperty("text", out var textProp) ? textProp.GetString() : "";

                                if (!string.IsNullOrEmpty(text))
                                {
                                    entries.Add(new TranscriptEntry
                                    {
                                        SpeakerIndex = speakerIndex,
                                        StartTime = startTime,
                                        Text = text.Trim()
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return entries.OrderBy(e => e.StartTime).ToList();
        }

        private string FormatTranscript(List<TranscriptEntry> entries, Dictionary<int, string> speakerMap)
        {
            var result = new List<string>();

            foreach (var entry in entries)
            {
                var speakerName = speakerMap.TryGetValue(entry.SpeakerIndex, out var name) 
                    ? name 
                    : $"Speaker {entry.SpeakerIndex}";

                var timestamp = FormatTimestamp(entry.StartTime);
                result.Add($"{speakerName} | {timestamp}");
                result.Add(entry.Text);
            }

            return string.Join(Environment.NewLine, result);
        }

        private string FormatTimestamp(double seconds)
        {
            var timespan = TimeSpan.FromSeconds(seconds);
            return $"{(int)timespan.TotalMinutes:D2}:{timespan.Seconds:D2}";
        }
    }

    public class TranscriptEntry
    {
        public int SpeakerIndex { get; set; }
        public double StartTime { get; set; }
        public string Text { get; set; } = string.Empty;
    }
} 