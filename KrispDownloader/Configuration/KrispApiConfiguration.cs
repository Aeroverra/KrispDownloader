namespace KrispDownloader.Configuration
{
    public class KrispApiConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string BearerToken { get; set; } = string.Empty;

        // Output controls
        public bool SaveRecordings { get; set; } = true;
        public bool SaveTranscripts { get; set; } = true;
        public bool SaveMeetingDetails { get; set; } = true;

        public string RecordingsOutput { get; set; } = "Krisp.AI Data Export/Recordings";
        public string TranscriptsOutput { get; set; } = "Krisp.AI Data Export/Transcripts";
        public string MeetingDetailsOutput { get; set; } = "Krisp.AI Data Export/MeetingDetails";
    }
}
