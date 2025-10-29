using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VimeoVideoUploader.Models;

namespace VimeoVideoUploader.Services;

/// <summary>
/// Handles video uploads to Vimeo using their API
/// </summary>
public class VideoUploader : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Logger _logger;
    private readonly AppConfig _config;
    private const string VimeoApiBaseUrl = "https://api.vimeo.com";
    private bool _disposed = false;

    public VideoUploader(AppConfig config, Logger logger)
    {
        _config = config;
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", config.VimeoAccessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Scans the video directory and returns a list of video files
    /// </summary>
    /// <returns>List of video file paths</returns>
    public List<string> GetVideoFiles()
    {
        try
        {
            var videoFiles = Directory.GetFiles(_config.VideoDirectory)
                .Where(file => _config.SupportedExtensions.Contains(
                    Path.GetExtension(file).ToLowerInvariant()))
                .ToList();

            _logger.LogInfo($"Found {videoFiles.Count} video file(s) in {_config.VideoDirectory}");
            return videoFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error scanning video directory: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Uploads a video file to Vimeo with retry logic
    /// </summary>
    /// <param name="videoFilePath">Path to the video file</param>
    /// <returns>UploadedVideo record with local filename and Vimeo URL</returns>
    public async Task<UploadedVideo?> UploadVideoAsync(string videoFilePath)
    {
        var fileName = Path.GetFileName(videoFilePath);
        var fileInfo = new FileInfo(videoFilePath);

        _logger.LogInfo($"Starting upload for: {fileName} (Size: {fileInfo.Length} bytes)");

        for (int attempt = 1; attempt <= _config.MaxRetryAttempts; attempt++)
        {
            try
            {
                // Step 1: Create the video object on Vimeo
                var uploadResponse = await CreateVimeoVideoAsync(fileInfo.Length, fileName);
                
                if (uploadResponse?.Upload?.UploadLink == null)
                {
                    _logger.LogError($"Failed to get upload link for {fileName}");
                    continue;
                }

                // Step 2: Upload the video file
                var uploadSuccess = await UploadVideoFileAsync(
                    videoFilePath, 
                    uploadResponse.Upload.UploadLink,
                    fileInfo.Length);

                if (!uploadSuccess)
                {
                    _logger.LogError($"Failed to upload video file for {fileName}");
                    continue;
                }

                // Step 3: Return the result
                var vimeoUrl = $"https://vimeo.com{uploadResponse.Uri.Replace("/videos/", "/")}";
                if (!string.IsNullOrEmpty(uploadResponse.Link))
                {
                    vimeoUrl = uploadResponse.Link;
                }

                var uploadedVideo = new UploadedVideo
                {
                    LocalVideo = fileName,
                    VimeoUrl = vimeoUrl
                };

                _logger.LogInfo($"Successfully uploaded {fileName} to {vimeoUrl}");
                return uploadedVideo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Upload attempt {attempt} failed for {fileName}: {ex.Message}", ex);
                
                if (attempt < _config.MaxRetryAttempts)
                {
                    _logger.LogInfo($"Retrying in {_config.RetryDelayMs}ms...");
                    await Task.Delay(_config.RetryDelayMs);
                }
            }
        }

        _logger.LogError($"All retry attempts exhausted for {fileName}");
        return null;
    }

    /// <summary>
    /// Creates a video object on Vimeo and gets the upload link
    /// </summary>
    private async Task<VimeoUploadResponse?> CreateVimeoVideoAsync(long fileSize, string fileName)
    {
        var url = $"{VimeoApiBaseUrl}/me/videos";
        
        var requestBody = new
        {
            upload = new
            {
                approach = "tus",
                size = fileSize
            },
            name = Path.GetFileNameWithoutExtension(fileName)
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _logger.LogRequest("POST", url, $"Content-Type: application/json, Authorization: Bearer ***");
        
        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        
        _logger.LogResponse((int)response.StatusCode, responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to create video on Vimeo: {response.StatusCode}");
            return null;
        }

        return JsonSerializer.Deserialize<VimeoUploadResponse>(responseBody);
    }

    /// <summary>
    /// Uploads the video file using TUS protocol
    /// </summary>
    private async Task<bool> UploadVideoFileAsync(string videoFilePath, string uploadLink, long fileSize)
    {
        try
        {
            using var fileStream = File.OpenRead(videoFilePath);
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");
            streamContent.Headers.ContentLength = fileSize;
            streamContent.Headers.Add("Tus-Resumable", "1.0.0");
            streamContent.Headers.Add("Upload-Offset", "0");

            _logger.LogRequest("PATCH", uploadLink, "Tus-Resumable: 1.0.0, Upload-Offset: 0");
            
            var response = await _httpClient.PatchAsync(uploadLink, streamContent);
            
            _logger.LogResponse((int)response.StatusCode);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading video file: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Disposes the HttpClient and releases resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
            _disposed = true;
        }
    }
}
