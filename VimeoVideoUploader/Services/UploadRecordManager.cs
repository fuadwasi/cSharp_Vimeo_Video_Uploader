using System.Text.Json;
using VimeoVideoUploader.Models;

namespace VimeoVideoUploader.Services;

/// <summary>
/// Manages reading and writing uploaded video records to JSON file
/// </summary>
public class UploadRecordManager
{
    private readonly string _recordFilePath;
    private readonly Logger _logger;

    public UploadRecordManager(string recordFilePath, Logger logger)
    {
        _recordFilePath = recordFilePath;
        _logger = logger;
    }

    /// <summary>
    /// Reads existing upload records from the JSON file
    /// </summary>
    /// <returns>List of uploaded video records</returns>
    public async Task<List<UploadedVideo>> ReadRecordsAsync()
    {
        try
        {
            if (!File.Exists(_recordFilePath))
            {
                _logger.LogInfo($"No existing upload records found at {_recordFilePath}");
                return new List<UploadedVideo>();
            }

            var jsonContent = await File.ReadAllTextAsync(_recordFilePath);
            var records = JsonSerializer.Deserialize<List<UploadedVideo>>(jsonContent);

            if (records == null)
            {
                _logger.LogInfo("Upload records file is empty or invalid");
                return new List<UploadedVideo>();
            }

            _logger.LogInfo($"Loaded {records.Count} existing upload record(s)");
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading upload records: {ex.Message}", ex);
            return new List<UploadedVideo>();
        }
    }

    /// <summary>
    /// Writes upload records to the JSON file
    /// </summary>
    /// <param name="records">List of uploaded video records</param>
    public async Task WriteRecordsAsync(List<UploadedVideo> records)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var jsonContent = JsonSerializer.Serialize(records, options);
            await File.WriteAllTextAsync(_recordFilePath, jsonContent);

            _logger.LogInfo($"Saved {records.Count} upload record(s) to {_recordFilePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error writing upload records: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Adds a new upload record and saves to file
    /// </summary>
    /// <param name="record">Upload record to add</param>
    public async Task AddRecordAsync(UploadedVideo record)
    {
        var records = await ReadRecordsAsync();
        records.Add(record);
        await WriteRecordsAsync(records);
    }

    /// <summary>
    /// Checks if a video file has already been uploaded
    /// </summary>
    /// <param name="fileName">Video filename to check</param>
    /// <returns>True if already uploaded, false otherwise</returns>
    public async Task<bool> IsVideoUploadedAsync(string fileName)
    {
        var records = await ReadRecordsAsync();
        return records.Any(r => r.LocalVideo.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }
}
