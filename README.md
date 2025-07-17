# Krisp Transcript Downloader

A C# console application that downloads all your Krisp.ai meeting transcripts.

## Features

- Downloads all available transcripts from your Krisp.ai account
- Handles pagination automatically to get all meetings
- Saves transcripts with descriptive filenames including date and meeting name
- Provides detailed logging of the download process
- Respectful API usage with delays between requests

## Setup

1. **Get your Krisp.ai Bearer Token**
   - Log into your Krisp.ai account
   - Open browser developer tools (F12)
   - Go to the Network tab
   - Make a request to the Krisp API (like viewing a meeting)
   - Find the Authorization header in the request and copy the Bearer token

2. **Configure the Application**
   - Open `appsettings.json`
   - Replace `"your-bearer-token-here"` with your actual bearer token
   - Optionally change the `DownloadDirectory` path (defaults to "Downloads")

3. **Run the Application**
   ```bash
   cd KrispDownloader
   dotnet run
   ```

## Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "KrispApi": {
    "BaseUrl": "https://api.krisp.ai/v2",
    "BearerToken": "your-bearer-token-here"
  },
  "DownloadDirectory": "Downloads"
}
```

## Output

- Transcripts are saved as JSON files in the specified download directory
- Files are named with the format: `{date}_{meeting-name}_{meeting-id}.json`
- The application will create the download directory if it doesn't exist
- Detailed logs show the progress of the download process

## What Gets Downloaded

- The application fetches all meetings from your Krisp.ai account
- Only meetings with available transcripts (status "uploaded" or "ready") are processed
- Each transcript is saved as a complete JSON response from the Krisp API
- The JSON includes transcript text, speakers, timestamps, and other meeting metadata

## Requirements

- .NET 9.0 or later
- Valid Krisp.ai account with meetings that have transcripts

## Notes

- The application runs once and then exits (it's not a continuous service)
- There's a small delay between API requests to be respectful to the Krisp.ai servers
- If a transcript fails to download, the application will continue with the next one
- All activity is logged to the console for monitoring progress
