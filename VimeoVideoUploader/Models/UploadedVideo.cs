namespace VimeoVideoUploader.Models;

/// <summary>
/// Model representing an uploaded video record
/// </summary>
public class UploadedVideo
{
    /// <summary>
    /// Local video filename
    /// </summary>
    public string LocalVideo { get; set; } = string.Empty;

    /// <summary>
    /// Vimeo URL of the uploaded video
    /// </summary>
    public string VimeoUrl { get; set; } = string.Empty;
}
