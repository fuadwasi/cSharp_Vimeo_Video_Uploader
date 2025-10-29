namespace VimeoVideoUploader.Services;

/// <summary>
/// Handles logging of requests, responses, and events to a file
/// </summary>
public class Logger
{
    private readonly string _logFilePath;
    private readonly object _lock = new object();

    public Logger(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    /// <summary>
    /// Logs a message with timestamp to the log file
    /// </summary>
    /// <param name="message">Message to log</param>
    public void Log(string message)
    {
        lock (_lock)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Logs an HTTP request
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="url">Request URL</param>
    /// <param name="headers">Request headers (optional)</param>
    public void LogRequest(string method, string url, string? headers = null)
    {
        var message = $"REQUEST: {method} {url}";
        if (!string.IsNullOrEmpty(headers))
        {
            message += $"{Environment.NewLine}Headers: {headers}";
        }
        Log(message);
    }

    /// <summary>
    /// Logs an HTTP response
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="responseBody">Response body (optional)</param>
    public void LogResponse(int statusCode, string? responseBody = null)
    {
        var message = $"RESPONSE: Status {statusCode}";
        if (!string.IsNullOrEmpty(responseBody))
        {
            message += $"{Environment.NewLine}Body: {responseBody}";
        }
        Log(message);
    }

    /// <summary>
    /// Logs an error
    /// </summary>
    /// <param name="error">Error message</param>
    /// <param name="exception">Exception (optional)</param>
    public void LogError(string error, Exception? exception = null)
    {
        var message = $"ERROR: {error}";
        if (exception != null)
        {
            message += $"{Environment.NewLine}Exception: {exception.Message}{Environment.NewLine}StackTrace: {exception.StackTrace}";
        }
        Log(message);
    }

    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="info">Information message</param>
    public void LogInfo(string info)
    {
        Log($"INFO: {info}");
    }
}
