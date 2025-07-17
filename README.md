# Krisp Transcript Downloader

A .NET console application that downloads all your Krisp.ai meeting transcripts using the Krisp API.

## Features

- 📋 Fetches all meetings from your Krisp.ai account with pagination
- 📝 Downloads transcripts for meetings that have them available
- 🔧 Configurable via `appsettings.json`
- 📊 Detailed logging of the download process
- 💾 Saves transcripts with organized filenames
- 🛡️ Rate limiting and respectful API usage

## Setup

1. **Get your Krisp Bearer Token**
   - Open your browser and log into [app.krisp.ai](https://app.krisp.ai)
   - Open Developer Tools (F12)
   - Go to Network tab and refresh the page
   - Look for API requests to `api.krisp.ai`
   - Find the `Authorization` header with format `Bearer xxxxxxxx`
   - Copy the token (everything after "Bearer ")

2. **Configure the application**
   - Open `appsettings.json`
   - Replace `YOUR_BEARER_TOKEN_HERE` with your actual bearer token
   - Optionally change the download directory path

3. **Build and run**
   ```bash
   dotnet build
   dotnet run
   ```

## Configuration

The `appsettings.json` file contains the following settings:

```json
{
  "KrispApi": {
    "BearerToken": "YOUR_BEARER_TOKEN_HERE",
    "BaseUrl": "https://api.krisp.ai/v2",
    "DownloadDirectory": "./downloads"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Usage

When you run the application, you'll see a menu with options:

1. **Download all transcripts** - Fetches all meetings and attempts to download their transcripts
2. **List meetings only** - Shows a preview of your meetings without downloading
3. **Exit** - Quit the application

## How It Works

1. **Fetch Meetings**: The application calls the `/meetings/list` endpoint with pagination to get all your meetings
2. **Filter Transcripts**: Only meetings with uploaded transcripts are processed
3. **Download Attempts**: For each meeting, the application:
   - Calls the `/actions/ui_download_notes` endpoint (as specified)
   - Tries alternative API endpoints to get transcript content:
     - `/meetings/{id}/transcript`
     - `/meetings/{id}/notes`
     - `/meetings/{id}/export`
4. **Save Files**: Successfully downloaded content is saved with descriptive filenames

## Known Limitations & Blob URL Challenge

⚠️ **Important Note**: The Krisp API's `/actions/ui_download_notes` endpoint returns a success response but doesn't provide a direct download URL. In the browser, transcripts are downloaded via blob URLs (like `blob:https://app.krisp.ai/...`), which are generated client-side.

### Current Workarounds

The application attempts several strategies to get transcript content:
1. Direct API endpoints for transcript data
2. Notes endpoints that might contain transcript information
3. Export endpoints that could provide downloadable content

### Potential Solutions

If the current approaches don't work, you may need to:

1. **Investigate Network Traffic**: 
   - Use browser dev tools to monitor all network requests when downloading a transcript manually
   - Look for additional API calls that provide the actual download URL

2. **Check for Additional Endpoints**:
   - The Krisp API might have other endpoints like `/meetings/{id}/download` or similar
   - Try different variations of export/download endpoints

3. **Browser Automation**: 
   - Consider using tools like Selenium or Playwright to automate the browser download process
   - This would handle the blob URL generation automatically

4. **Contact Krisp Support**:
   - Ask about programmatic access to transcript downloads
   - Request documentation for the complete download API flow

## File Naming Convention

Downloaded files are saved with the following format:
```
{meeting_id}_{sanitized_meeting_name}_{date}.{extension}
```

Example: `502a6948b75648d0a4837ba282727f03_firefox_meeting_July_17_2025-07-16.transcript.json`

## Logging

The application provides detailed logging to help you understand what's happening:
- Meeting fetch progress
- Download attempts and results
- Error messages and debugging information
- Success/failure statistics

## Requirements

- .NET 9.0 or later
- Valid Krisp.ai account with meetings
- Bearer token from your Krisp session

## Contributing

If you discover working API endpoints for direct transcript downloads, please update the `TryAlternativeDownloadApproaches` method in `KrispDownloaderService.cs`.

## Disclaimer

This tool is for personal use with your own Krisp.ai account. Respect Krisp's terms of service and API rate limits. The bearer token gives access to your account data, so keep it secure.
