using System.Text.Json;
using VimeoVideoUploader.Models;

namespace VimeoVideoUploader.Services;

/// <summary>
/// Handles reading and writing configuration from/to JSON files
/// </summary>
public class ConfigReader
{
    private readonly string _configFilePath;
    private readonly Logger _logger;

    public ConfigReader(string configFilePath, Logger logger)
    {
        _configFilePath = configFilePath;
        _logger = logger;
    }

    /// <summary>
    /// Reads the application configuration from the JSON file
    /// </summary>
    /// <returns>AppConfig object</returns>
    public async Task<AppConfig> ReadConfigAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogError($"Configuration file not found: {_configFilePath}");
                throw new FileNotFoundException($"Configuration file not found: {_configFilePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<AppConfig>(jsonContent);

            if (config == null)
            {
                _logger.LogError("Failed to deserialize configuration file");
                throw new InvalidOperationException("Failed to deserialize configuration file");
            }

            _logger.LogInfo($"Configuration loaded successfully from {_configFilePath}");
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reading configuration file: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    /// <param name="config">Configuration to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool ValidateConfig(AppConfig config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.VimeoAccessToken))
        {
            errors.Add("VimeoAccessToken is required");
        }

        if (string.IsNullOrWhiteSpace(config.VideoDirectory))
        {
            errors.Add("VideoDirectory is required");
        }
        else if (!Directory.Exists(config.VideoDirectory))
        {
            errors.Add($"VideoDirectory does not exist: {config.VideoDirectory}");
        }

        if (config.SupportedExtensions == null || config.SupportedExtensions.Length == 0)
        {
            errors.Add("At least one supported extension is required");
        }

        if (config.MaxRetryAttempts < 0)
        {
            errors.Add("MaxRetryAttempts must be non-negative");
        }

        if (config.RetryDelayMs < 0)
        {
            errors.Add("RetryDelayMs must be non-negative");
        }

        if (errors.Any())
        {
            foreach (var error in errors)
            {
                _logger.LogError($"Configuration validation error: {error}");
            }
            return false;
        }

        _logger.LogInfo("Configuration validated successfully");
        return true;
    }
}
