# Krisp Downloader / Exporter
Export your Krisp.ai meeting data (details, transcripts, recordings) to local files for your own records.

## Quick Start
1) Download the latest release asset for your platform (names include the RID, e.g., `KrispDownloader-win-x64.exe`, `KrispDownloader-osx-arm64`, etc.).
2) Run it once. It will create `appsettings.json` next to the executable and exit.
3) Edit `appsettings.json`:
   - Set `BearerToken` to your Krisp token (see “Get your Krisp bearer token” below).
   - Optional: tweak output paths or disable parts (`SaveRecordings`, `SaveTranscripts`, `SaveMeetingDetails`).
4) Run it again to export meetings (details JSON, transcript JSON + formatted text, and recordings).

## Get your Krisp bearer token (step-by-step)
1) Go to https://app.krisp.ai and log in.
2) Make sure you’re on the **Meetings** tab (list of meetings visible).
3) Open browser DevTools → **Network** tab.
4) Click any meeting in the list; a request with a random-looking id will appear.
5) Select that request → **Headers** → under **Request Headers**, find `Authorization`.
6) Copy the value after `Bearer ` and paste it into `KrispApi.BearerToken` in `appsettings.json`.

Tip: You can filter Network requests by typing “recording” or “meetings” to find it faster. The attached screenshots (meetings list and Network tab with Authorization header) show what to look for.

## What gets exported
- Meeting details (formatted JSON)  
- Transcripts (raw JSON + formatted text)  
- Recordings (mp3) when available  
- Filenames include timestamp + meeting name + id; directories are auto-created.

## Configuration (created on first run)
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