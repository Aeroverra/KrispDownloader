using System.Text.Json;

namespace Aeroverra.KrispDownloader.Services
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

                var speakerMap = ExtractSpeakerMap(root);
                var transcriptContent = ExtractTranscriptContent(root);

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

            if (root.TryGetProperty("resources", out var resources) && resources.ValueKind == JsonValueKind.Array)
            {
                foreach (var resource in resources.EnumerateArray())
                {
                    if (resource.TryGetProperty("resource_type", out var resourceType)
                        && resourceType.GetString() == "speakers_map"
                        && resource.TryGetProperty("content", out var content)
                        && content.TryGetProperty("data", out var speakerData))
                    {
                        foreach (var speakerProperty in speakerData.EnumerateObject())
                        {
                            if (int.TryParse(speakerProperty.Name, out var speakerIndex))
                            {
                                var speaker = speakerProperty.Value;
                                if (speaker.TryGetProperty("person", out var person))
                                {
                                    var firstName = person.TryGetProperty("first_name", out var fn) ? fn.GetString() : "";
                                    var lastName = person.TryGetProperty("last_name", out var ln) && ln.ValueKind != JsonValueKind.Null ? ln.GetString() : "";

                                    var displayName = !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName)
                                        ? $"{firstName} {lastName}"
                                        : !string.IsNullOrEmpty(firstName) ? firstName
                                        : !string.IsNullOrEmpty(lastName) ? lastName
                                        : $"Speaker {speakerIndex}";

                                    speakerMap[speakerIndex] = displayName;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            return speakerMap;
        }

        private List<TranscriptEntry> ExtractTranscriptContent(JsonElement root)
        {
            var entries = new List<TranscriptEntry>();

            var transcriptBlock = FindBlockByType(root, "transcript");
            if (transcriptBlock == null)
            {
                return entries;
            }

            var block = transcriptBlock.Value;
            if (block.TryGetProperty("content", out var content)
                && content.TryGetProperty("speech_data", out var speechData)
                && speechData.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in speechData.EnumerateArray())
                {
                    if (item.TryGetProperty("speakerIndex", out var speakerIndexProp)
                        && item.TryGetProperty("speech", out var speech))
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

            return entries.OrderBy(e => e.StartTime).ToList();
        }

        private JsonElement? FindBlockByType(JsonElement element, string blockType)
        {
            if (element.TryGetProperty("block_type", out var bt) && bt.GetString() == blockType)
            {
                return element;
            }

            if (element.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in children.EnumerateArray())
                {
                    var found = FindBlockByType(child, blockType);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
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
