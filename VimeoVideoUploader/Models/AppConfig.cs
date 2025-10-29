namespace VimeoVideoUploader.Models;

/// <summary>
/// Configuration model for the application
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Vimeo API access token for authentication
    /// </summary>
    public string VimeoAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Directory path containing videos to upload
    /// </summary>
    public string VideoDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Supported video file extensions
    /// </summary>
    public string[] SupportedExtensions { get; set; } = { ".mp4", ".mov", ".avi" };

    /// <summary>
    /// Path to the log file
    /// </summary>
    public string LogFilePath { get; set; } = "upload_log.txt";

    /// <summary>
    /// Path to the uploaded videos JSON file
    /// </summary>
    public string UploadedVideosFilePath { get; set; } = "uploaded_videos.json";

    /// <summary>
    /// Maximum retry attempts for failed uploads
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay in milliseconds between retry attempts
    /// </summary>
    public int RetryDelayMs { get; set; } = 2000;
}
