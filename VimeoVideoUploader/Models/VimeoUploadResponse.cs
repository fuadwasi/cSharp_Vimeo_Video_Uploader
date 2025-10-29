using System.Text.Json.Serialization;

namespace VimeoVideoUploader.Models;

/// <summary>
/// Response model from Vimeo upload API
/// </summary>
public class VimeoUploadResponse
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("upload")]
    public VimeoUploadInfo? Upload { get; set; }
}

/// <summary>
/// Upload information from Vimeo API
/// </summary>
public class VimeoUploadInfo
{
    [JsonPropertyName("upload_link")]
    public string UploadLink { get; set; } = string.Empty;

    [JsonPropertyName("approach")]
    public string Approach { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }
}
