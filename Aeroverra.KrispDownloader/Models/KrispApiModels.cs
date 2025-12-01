using Newtonsoft.Json;

namespace Aeroverra.KrispDownloader.Models
{
    public class MeetingsListResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data")]
        public MeetingsData Data { get; set; } = new();

        [JsonProperty("req_id")]
        public string ReqId { get; set; } = string.Empty;
    }

    public class MeetingsData
    {
        [JsonProperty("rows")]
        public List<Meeting> Rows { get; set; } = new();

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class Meeting
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonProperty("started_at")]
        public string StartedAt { get; set; } = string.Empty;

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("app_name")]
        public string AppName { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("resources")]
        public Resources Resources { get; set; } = new();

        [JsonProperty("speakers")]
        public List<Speaker> Speakers { get; set; } = new();

        [JsonProperty("user_interactions")]
        public UserInteractions UserInteractions { get; set; } = new();

        [JsonProperty("is_demo")]
        public bool IsDemo { get; set; }

        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }
    }

    public class Resources
    {
        [JsonProperty("transcript")]
        public Transcript Transcript { get; set; } = new();

        [JsonProperty("recording")]
        public bool Recording { get; set; }

        [JsonProperty("recordings")]
        public List<Recording> Recordings { get; set; } = new();

        [JsonProperty("meeting_notes")]
        public MeetingNotes MeetingNotes { get; set; } = new();
    }

    public class Transcript
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("processor")]
        public string Processor { get; set; } = string.Empty;
    }

    public class Recording
    {
        [JsonProperty("mime_type")]
        public string MimeType { get; set; } = string.Empty;

        [JsonProperty("size")]
        public long Size { get; set; }
    }

    // Detailed meeting response (used for recording download and transcript parsing)
    public class MeetingDetailsResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data")]
        public MeetingDetailsData Data { get; set; } = new();

        [JsonProperty("req_id")]
        public string ReqId { get; set; } = string.Empty;
    }

    public class MeetingDetailsData
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonProperty("resources")]
        public MeetingResources Resources { get; set; } = new();
    }

    public class MeetingResources
    {
        [JsonProperty("transcript")]
        public TranscriptDetail Transcript { get; set; } = new();

        [JsonProperty("recording")]
        public RecordingDetail? Recording { get; set; }

        [JsonProperty("recordings")]
        public List<RecordingDetail> Recordings { get; set; } = new();
    }

    public class TranscriptDetail
    {
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("method")]
        public string? Method { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }

        [JsonProperty("processor")]
        public string? Processor { get; set; }

        [JsonProperty("content")]
        public string? Content { get; set; }
    }

    public class RecordingDetail
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("size")]
        public long? Size { get; set; }

        [JsonProperty("created_at")]
        public string? CreatedAt { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("mime_type")]
        public string? MimeType { get; set; }

        [JsonProperty("capture_type")]
        public string? CaptureType { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }

    public class MeetingDetailsResult
    {
        public string RawJson { get; set; } = string.Empty;
        public MeetingDetailsResponse? Parsed { get; set; }
    }

    public class MeetingNotes
    {
        [JsonProperty("action_items")]
        public ActionItems? ActionItems { get; set; }

        [JsonProperty("key_points")]
        public KeyPoints? KeyPoints { get; set; }
    }

    public class ActionItems
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class KeyPoints
    {
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class Speaker
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonProperty("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("photo")]
        public string Photo { get; set; } = string.Empty;
    }

    public class UserInteractions
    {
        [JsonProperty("read")]
        public bool Read { get; set; }

        [JsonProperty("starred")]
        public bool Starred { get; set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }
    }

    public class MeetingsListRequest
    {
        [JsonProperty("sort")]
        public string Sort { get; set; } = "desc";

        [JsonProperty("sortKey")]
        public string SortKey { get; set; } = "created_at";

        [JsonProperty("page")]
        public int Page { get; set; } = 1;

        [JsonProperty("limit")]
        public int Limit { get; set; } = 250;

        [JsonProperty("starred")]
        public bool Starred { get; set; } = false;
    }
}
