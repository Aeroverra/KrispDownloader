# Krisp Downloader (Transcripts + Recordings)

## Quick Start (binary)
1) Download the latest release asset for your platform (names include the RID, e.g., `KrispDownloader-win-x64.exe`, `KrispDownloader-osx-arm64`, etc.).
2) Run it once. On first run it will create `appsettings.json` next to the executable and exit.
3) Edit `appsettings.json`:
   - Set `BearerToken` to your Krisp token (see “Get your bearer token” below).
   - Optionally adjust output paths and toggles (`SaveRecordings`, `SaveTranscripts`, `SaveMeetingDetails`).
4) Run it again to download meeting details, transcripts, and recordings.

## Quick Start (source)
```bash
dotnet restore Aeroverra.KrispDownloader/Aeroverra.KrispDownloader.csproj
dotnet run --project Aeroverra.KrispDownloader/Aeroverra.KrispDownloader.csproj
```
- First run writes `appsettings.json`, then exits. Edit it and rerun.

## Get your Krisp bearer token
- Log into Krisp in your browser.
- Open dev tools (F12) → Network.
- Trigger a Krisp API request (e.g., open a meeting).
- Copy the `Authorization: Bearer ...` header value.
- Paste into `KrispApi.BearerToken` in `appsettings.json`.

## What it does
- Lists all meetings, pulls full meeting details JSON, and saves:
  - Meeting details (formatted JSON).
  - Transcript (pretty JSON + formatted text).
  - Recording audio (mp3) when available.
- Respects API (small delay, pagination handled).
- Logs progress to console.

## Outputs & config
`appsettings.json` keys (created on first run):
```json
{
  "KrispApi": {
    "BaseUrl": "https://api.krisp.ai",
    "BearerToken": "your-bearer-token-here",
    "SaveRecordings": true,
    "SaveTranscripts": true,
    "SaveMeetingDetails": true,
    "RecordingsOutput": "Krisp.AI Data Export/Recordings",
    "TranscriptsOutput": "Krisp.AI Data Export/Transcripts",
    "MeetingDetailsOutput": "Krisp.AI Data Export/MeetingDetails"
  }
}
```
- Filenames include timestamp, meeting name, and meeting id.
- Directories are created automatically.

## Supported release artifacts
- Windows: `win-x64`, `win-x86`
- macOS: `osx-x64`, `osx-arm64`
- Linux: `linux-x64`, `linux-arm64`

## Requirements
- For source: .NET 10 SDK.
- For binaries: no runtime required (self-contained).
- Krisp account with access to meetings.

## Notes
- Runs once and exits; rerun anytime to fetch new meetings.
- If a download fails, processing continues with the next meeting.
