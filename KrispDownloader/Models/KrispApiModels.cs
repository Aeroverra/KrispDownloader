using System.Text.Json.Serialization;

namespace KrispDownloader.Models
{
    public class MeetingsListResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public MeetingsData Data { get; set; } = new();

        [JsonPropertyName("req_id")]
        public string ReqId { get; set; } = string.Empty;
    }

    public class MeetingsData
    {
        [JsonPropertyName("rows")]
        public List<Meeting> Rows { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class Meeting
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("started_at")]
        public string StartedAt { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("app_name")]
        public string AppName { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("resources")]
        public Resources Resources { get; set; } = new();

        [JsonPropertyName("speakers")]
        public List<Speaker> Speakers { get; set; } = new();

        [JsonPropertyName("user_interactions")]
        public UserInteractions UserInteractions { get; set; } = new();

        [JsonPropertyName("is_demo")]
        public bool IsDemo { get; set; }

        [JsonPropertyName("is_private")]
        public bool IsPrivate { get; set; }
    }

    public class Resources
    {
        [JsonPropertyName("transcript")]
        public Transcript Transcript { get; set; } = new();

        [JsonPropertyName("recording")]
        public bool Recording { get; set; }

        [JsonPropertyName("recordings")]
        public List<Recording> Recordings { get; set; } = new();

        [JsonPropertyName("meeting_notes")]
        public MeetingNotes MeetingNotes { get; set; } = new();
    }

    public class Transcript
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("processor")]
        public string Processor { get; set; } = string.Empty;
    }

    public class Recording
    {
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }

    // Detailed meeting response (used for recording download and transcript parsing)
    public class MeetingDetailsResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public MeetingDetailsData Data { get; set; } = new();

        [JsonPropertyName("req_id")]
        public string ReqId { get; set; } = string.Empty;
    }

    public class MeetingDetailsData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("resources")]
        public MeetingResources Resources { get; set; } = new();
    }

    public class MeetingResources
    {
        [JsonPropertyName("transcript")]
        public TranscriptDetail Transcript { get; set; } = new();

        [JsonPropertyName("recording")]
        public RecordingDetail? Recording { get; set; }

        [JsonPropertyName("recordings")]
        public List<RecordingDetail> Recordings { get; set; } = new();
    }

    public class TranscriptDetail
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("processor")]
        public string? Processor { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class RecordingDetail
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        [JsonPropertyName("capture_type")]
        public string? CaptureType { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class MeetingDetailsResult
    {
        public string RawJson { get; set; } = string.Empty;
        public MeetingDetailsResponse? Parsed { get; set; }
    }

    public class MeetingNotes
    {
        [JsonPropertyName("action_items")]
        public ActionItems? ActionItems { get; set; }

        [JsonPropertyName("key_points")]
        public KeyPoints? KeyPoints { get; set; }
    }

    public class ActionItems
    {
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class KeyPoints
    {
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class Speaker
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("photo")]
        public string Photo { get; set; } = string.Empty;
    }

    public class UserInteractions
    {
        [JsonPropertyName("read")]
        public bool Read { get; set; }

        [JsonPropertyName("starred")]
        public bool Starred { get; set; }

        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }
    }

    public class MeetingsListRequest
    {
        [JsonPropertyName("sort")]
        public string Sort { get; set; } = "desc";

        [JsonPropertyName("sortKey")]
        public string SortKey { get; set; } = "created_at";

        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 250;

        [JsonPropertyName("starred")]
        public bool Starred { get; set; } = false;
    }
} 
