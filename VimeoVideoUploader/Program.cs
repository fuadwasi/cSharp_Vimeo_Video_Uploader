using VimeoVideoUploader.Services;

namespace VimeoVideoUploader;

/// <summary>
/// Main program entry point
/// </summary>
class Program
{
    private const string ConfigFilePath = "config.json";

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== Vimeo Video Uploader ===");
        Console.WriteLine();

        // Initialize logger first
        var logger = new Logger("upload_log.txt");
        logger.LogInfo("Application started");

        try
        {
            // Read and validate configuration
            var configReader = new ConfigReader(ConfigFilePath, logger);
            var config = await configReader.ReadConfigAsync();

            if (!configReader.ValidateConfig(config))
            {
                Console.WriteLine("Configuration validation failed. Please check the log file for details.");
                logger.LogError("Configuration validation failed");
                return 1;
            }

            Console.WriteLine($"Configuration loaded successfully");
            Console.WriteLine($"Video Directory: {config.VideoDirectory}");
            Console.WriteLine($"Supported Extensions: {string.Join(", ", config.SupportedExtensions)}");
            Console.WriteLine();

            // Initialize services
            var uploadRecordManager = new UploadRecordManager(config.UploadedVideosFilePath, logger);
            var videoUploader = new VideoUploader(config, logger);

            // Get video files
            var videoFiles = videoUploader.GetVideoFiles();

            if (videoFiles.Count == 0)
            {
                Console.WriteLine("No video files found in the specified directory.");
                logger.LogInfo("No video files found");
                return 0;
            }

            Console.WriteLine($"Found {videoFiles.Count} video file(s) to process");
            Console.WriteLine();

            // Process each video
            int successCount = 0;
            int skippedCount = 0;
            int failedCount = 0;

            for (int i = 0; i < videoFiles.Count; i++)
            {
                var videoFile = videoFiles[i];
                var fileName = Path.GetFileName(videoFile);

                Console.WriteLine($"[{i + 1}/{videoFiles.Count}] Processing: {fileName}");

                // Check if already uploaded
                if (await uploadRecordManager.IsVideoUploadedAsync(fileName))
                {
                    Console.WriteLine($"  ⊘ Skipped (already uploaded)");
                    logger.LogInfo($"Skipped {fileName} - already uploaded");
                    skippedCount++;
                    continue;
                }

                // Upload the video
                Console.Write($"  ↑ Uploading... ");
                var result = await videoUploader.UploadVideoAsync(videoFile);

                if (result != null)
                {
                    Console.WriteLine("✓ Success");
                    Console.WriteLine($"  Vimeo URL: {result.VimeoUrl}");
                    
                    await uploadRecordManager.AddRecordAsync(result);
                    successCount++;
                }
                else
                {
                    Console.WriteLine("✗ Failed");
                    logger.LogError($"Upload failed for {fileName}");
                    failedCount++;
                }

                Console.WriteLine();
            }

            // Display summary
            Console.WriteLine("=== Upload Summary ===");
            Console.WriteLine($"Total files: {videoFiles.Count}");
            Console.WriteLine($"Successful: {successCount}");
            Console.WriteLine($"Skipped: {skippedCount}");
            Console.WriteLine($"Failed: {failedCount}");
            Console.WriteLine();
            Console.WriteLine($"Upload records saved to: {config.UploadedVideosFilePath}");
            Console.WriteLine($"Log file: {config.LogFilePath}");

            logger.LogInfo($"Application completed - Success: {successCount}, Skipped: {skippedCount}, Failed: {failedCount}");
            
            videoUploader.Dispose();
            return failedCount > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            logger.LogError($"Fatal error: {ex.Message}", ex);
            return 1;
        }
    }
}
