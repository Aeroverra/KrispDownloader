using Aeroverra.KrispDownloader.Models;
using Newtonsoft.Json.Linq;

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
                var root = JObject.Parse(jsonContent);

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

        private Dictionary<int, string> ExtractSpeakerMap(JObject root)
        {
            var speakerMap = new Dictionary<int, string>();

            if (root.TryGetValue("data", out var dataToken) &&
                dataToken is JObject data &&
                data.TryGetValue("speakers", out var speakersToken) &&
                speakersToken is JObject speakers &&
                speakers.TryGetValue("data", out var speakerDataToken) &&
                speakerDataToken is JObject speakerData)
            {
                foreach (var speakerProperty in speakerData.Properties())
                {
                    if (int.TryParse(speakerProperty.Name, out var speakerIndex) &&
                        speakerProperty.Value is JObject speaker &&
                        speaker.TryGetValue("person", out var personToken) &&
                        personToken is JObject person)
                    {
                        var firstName = person.Value<string>("first_name") ?? string.Empty;
                        var lastName = person.Value<string>("last_name") ?? string.Empty;

                        var displayName = !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName)
                            ? $"{firstName} {lastName}"
                            : !string.IsNullOrEmpty(firstName) ? firstName
                            : !string.IsNullOrEmpty(lastName) ? lastName
                            : $"Speaker {speakerIndex}";

                        speakerMap[speakerIndex] = displayName;
                    }
                }
            }

            return speakerMap;
        }

        private List<TranscriptEntry> ExtractTranscriptContent(JObject root)
        {
            var entries = new List<TranscriptEntry>();

            if (root.TryGetValue("data", out var dataToken) &&
                dataToken is JObject data &&
                data.TryGetValue("resources", out var resourcesToken) &&
                resourcesToken is JObject resources &&
                resources.TryGetValue("transcript", out var transcriptToken) &&
                transcriptToken is JObject transcript &&
                transcript.TryGetValue("content", out var contentToken))
            {
                var contentString = contentToken?.ToString();
                if (!string.IsNullOrEmpty(contentString))
                {
                    var transcriptArray = JArray.Parse(contentString);
                    foreach (var item in transcriptArray.OfType<JObject>())
                    {
                        if (item.TryGetValue("speakerIndex", out var speakerIndexToken) &&
                            item.TryGetValue("speech", out var speechToken) &&
                            speechToken is JObject speech)
                        {
                            var speakerIndex = speakerIndexToken.Value<int>();
                            var startTime = speech.Value<double?>("start") ?? 0;
                            var text = speech.Value<string>("text") ?? string.Empty;

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
