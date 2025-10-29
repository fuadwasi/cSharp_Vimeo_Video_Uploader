# Vimeo Video Uploader

A C# .NET 8.0 desktop console application that uploads videos to Vimeo using their official API.

## Features

- 📁 Scans a directory for video files (.mp4, .mov, .avi)
- ⬆️ Uploads videos to Vimeo using the official API
- 💾 Stores upload records (local filename + Vimeo URL) in JSON format
- 📝 Maintains detailed log file with timestamps
- 🔄 Implements retry logic for failed uploads
- ✅ Validates video files before upload
- 🎯 Skips already uploaded videos
- ⚙️ Configurable via JSON file
- 🚀 Async/await for non-blocking operations

## Prerequisites

- .NET 8.0 SDK or later
- Vimeo account with API access
- Vimeo access token (see [Getting Started](#getting-started))

## Installation

1. Clone this repository:
```bash
git clone https://github.com/fuadwasi/cSharp_Vimeo_Video_Uploader.git
cd cSharp_Vimeo_Video_Uploader
```

2. Build the project:
```bash
cd VimeoVideoUploader
dotnet build
```

## Getting Started

### 1. Get Vimeo Access Token

1. Go to [Vimeo Developer](https://developer.vimeo.com/apps)
2. Create a new app or use an existing one
3. Generate a personal access token with the following scopes:
   - `upload` - Upload videos
   - `create` - Create videos
   - `edit` - Edit videos
4. Copy the access token for use in configuration

### 2. Configure the Application

Create a `config.json` file in the VimeoVideoUploader directory (use `config.json.example` as a template):

```json
{
  "VimeoAccessToken": "YOUR_VIMEO_ACCESS_TOKEN_HERE",
  "VideoDirectory": "/path/to/your/videos",
  "SupportedExtensions": [
    ".mp4",
    ".mov",
    ".avi"
  ],
  "LogFilePath": "upload_log.txt",
  "UploadedVideosFilePath": "uploaded_videos.json",
  "MaxRetryAttempts": 3,
  "RetryDelayMs": 2000
}
```

#### Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `VimeoAccessToken` | Your Vimeo API access token | **Required** |
| `VideoDirectory` | Directory containing videos to upload | **Required** |
| `SupportedExtensions` | Array of video file extensions to process | `[".mp4", ".mov", ".avi"]` |
| `LogFilePath` | Path to the log file | `upload_log.txt` |
| `UploadedVideosFilePath` | Path to the uploaded videos record file | `uploaded_videos.json` |
| `MaxRetryAttempts` | Number of retry attempts for failed uploads | `3` |
| `RetryDelayMs` | Delay in milliseconds between retries | `2000` |

### 3. Run the Application

```bash
dotnet run
```

Or run the compiled executable:

```bash
dotnet bin/Debug/net8.0/VimeoVideoUploader.dll
```

## Output Files

### uploaded_videos.json

Stores records of successfully uploaded videos:

```json
[
  {
    "local_video": "example1.mp4",
    "vimeo_url": "https://vimeo.com/123456789"
  },
  {
    "local_video": "example2.mp4",
    "vimeo_url": "https://vimeo.com/987654321"
  }
]
```

### upload_log.txt

Contains detailed logs with timestamps:

```
[2025-10-29 07:45:00] INFO: Application started
[2025-10-29 07:45:01] INFO: Configuration loaded successfully from config.json
[2025-10-29 07:45:01] INFO: Found 2 video file(s) in /path/to/videos
[2025-10-29 07:45:02] INFO: Starting upload for: example1.mp4 (Size: 5242880 bytes)
[2025-10-29 07:45:02] REQUEST: POST https://api.vimeo.com/me/videos
[2025-10-29 07:45:03] RESPONSE: Status 200
[2025-10-29 07:45:05] INFO: Successfully uploaded example1.mp4 to https://vimeo.com/123456789
```

## Project Structure

```
VimeoVideoUploader/
├── Models/
│   ├── AppConfig.cs              # Configuration model
│   ├── UploadedVideo.cs          # Upload record model
│   └── VimeoUploadResponse.cs    # Vimeo API response models
├── Services/
│   ├── ConfigReader.cs           # Configuration reader and validator
│   ├── Logger.cs                 # Logging service
│   ├── UploadRecordManager.cs    # Upload records manager
│   └── VideoUploader.cs          # Video upload service
├── Program.cs                    # Main entry point
├── config.json.example           # Example configuration file
└── VimeoVideoUploader.csproj     # Project file
```

## Architecture

The application follows a modular, service-oriented architecture:

- **Models**: Data structures for configuration and API responses
- **Services**: Reusable business logic components
  - `ConfigReader`: Reads and validates configuration
  - `Logger`: Thread-safe logging to file
  - `UploadRecordManager`: Manages upload history
  - `VideoUploader`: Handles Vimeo API integration
- **Program**: Orchestrates the upload workflow

## Error Handling

- Configuration validation before processing
- Retry logic with configurable attempts and delays
- Detailed error logging with stack traces
- Graceful handling of network failures
- Skips already uploaded videos to prevent duplicates

## Security

- Access token is read from configuration file (excluded from git via .gitignore)
- Token is not logged or displayed in console output
- Secure HTTPS communication with Vimeo API

## Extending the Application

The modular design makes it easy to extend:

- Add support for additional video formats
- Implement a GUI using WPF or Windows Forms
- Add scheduling capabilities for automated uploads
- Integrate with cloud storage services
- Add video metadata editing features
- Implement parallel uploads for better performance

## Troubleshooting

### "Configuration file not found"
- Ensure `config.json` exists in the same directory as the executable
- Check the file name and path

### "Failed to create video on Vimeo"
- Verify your access token is valid and has the correct permissions
- Check your Vimeo account upload quota
- Review the log file for detailed error messages

### "VideoDirectory does not exist"
- Ensure the path in config.json points to an existing directory
- Use absolute paths for clarity

## License

This project is open source and available under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and questions:
- Check the log file for detailed error messages
- Review [Vimeo API Documentation](https://developer.vimeo.com/api/upload/videos)
- Open an issue on GitHub