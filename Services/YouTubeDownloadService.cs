using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using QuikytLoader.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace QuikytLoader.Services;

/// <summary>
/// Service for downloading videos from YouTube using yt-dlp
/// </summary>
public partial class YouTubeDownloadService : IYouTubeDownloadService
{
    private readonly string _downloadDirectory;
    private readonly string _tempDirectory;

    public YouTubeDownloadService()
    {
        // Use ~/Downloads/QuikytLoader as default download directory
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _downloadDirectory = Path.Combine(homeDir, "Downloads", "QuikytLoader");

        // Use system temp directory for temporary files (thumbnails)
        _tempDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");
    }

    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format
    /// </summary>
    public async Task<DownloadResult> DownloadAsync(string url, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        EnsureDownloadDirectoryExists();
        EnsureTempDirectoryExists();

        ValidateUrl(url);

        var tempOutputPath = GenerateTempOutputPath();
        var arguments = BuildYtDlpArguments(url, tempOutputPath);

        await RunYtDlpAsync(arguments, progress, cancellationToken);

        var result = FindDownloadedFilesAndMove();
        Console.WriteLine($"Downloaded: {result.AudioFilePath}, Thumbnail: {result.ThumbnailPath ?? "none"}");
        CleanupTempFiles(result.AudioFilePath);

        return result;
    }

    /// <summary>
    /// Validates that the URL is not empty
    /// </summary>
    private static void ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be empty", nameof(url));
        }
    }

    /// <summary>
    /// Ensures the download directory exists
    /// </summary>
    private void EnsureDownloadDirectoryExists()
    {
        if (!Directory.Exists(_downloadDirectory))
        {
            Directory.CreateDirectory(_downloadDirectory);
        }
    }

    /// <summary>
    /// Ensures the temporary directory exists
    /// </summary>
    private void EnsureTempDirectoryExists()
    {
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }

    /// <summary>
    /// Generates the output file path template for yt-dlp in temp directory
    /// Uses %(title)s to get the actual video title
    /// Downloads to temp directory first, then moves to final location
    /// </summary>
    private string GenerateTempOutputPath()
    {
        // yt-dlp will automatically sanitize the title for filesystem
        return Path.Combine(_tempDirectory, "%(title)s");
    }

    /// <summary>
    /// Builds command-line arguments for yt-dlp
    /// Includes comprehensive metadata embedding for all available fields
    /// Maps YouTube metadata to MP3 ID3 tags (Artist, Album, Title, etc.)
    /// </summary>
    private static string BuildYtDlpArguments(string url, string outputPath)
    {
        return $"--extract-audio " +
               $"--audio-format mp3 " +
               $"--audio-quality 0 " +
               $"--output \"{outputPath}.%(ext)s\" " +
               $"--no-playlist " +
               $"--add-metadata " +
               $"--embed-thumbnail " +
               $"--parse-metadata \"%(title)s:%(meta_title)s\" " +
               $"--parse-metadata \"%(uploader)s:%(meta_artist)s\" " +
               $"--parse-metadata \"%(uploader)s:%(meta_album_artist)s\" " +
               $"--parse-metadata \"%(channel)s:%(meta_album)s\" " +
               $"--parse-metadata \"%(upload_date>%Y)s:%(meta_date)s\" " +
               $"--parse-metadata \"%(creator)s:%(meta_composer)s\" " +
               $"--parse-metadata \"%(uploader)s:%(meta_performer)s\" " +
               $"--parse-metadata \"%(description)s:%(meta_comment)s\" " +
               $"--parse-metadata \"%(channel)s:%(meta_publisher)s\" " +
               $"--parse-metadata \"%(webpage_url)s:%(meta_purl)s\" " +
               $"--parse-metadata \"%(genre)s:%(meta_genre)s\" " +
               $"--write-thumbnail " +
               $"--convert-thumbnails jpg " +
               $"--progress " +
               $"\"{url}\"";
    }

    /// <summary>
    /// Runs yt-dlp process and monitors progress
    /// </summary>
    private static async Task RunYtDlpAsync(string arguments, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var processInfo = CreateProcessInfo(arguments);

        using var process = new Process { StartInfo = processInfo };

        process.OutputDataReceived += (sender, e) => HandleOutput(e.Data, progress);
        process.ErrorDataReceived += (sender, e) => HandleOutput(e.Data, progress);

        StartProcess(process);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await WaitForProcessExit(process, cancellationToken);

        ValidateProcessSuccess(process);
    }

    /// <summary>
    /// Creates process start info for yt-dlp
    /// </summary>
    private static ProcessStartInfo CreateProcessInfo(string arguments)
    {
        return new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    /// <summary>
    /// Starts the process and validates it started successfully
    /// </summary>
    private static void StartProcess(Process process)
    {
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start yt-dlp process");
        }
    }

    /// <summary>
    /// Waits for the process to exit asynchronously
    /// Kills the process if cancellation is requested
    /// </summary>
    private static async Task WaitForProcessExit(Process process, CancellationToken cancellationToken)
    {
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Kill the yt-dlp process if cancellation is requested
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(); // Wait for the process to fully exit
            }
            throw; // Re-throw to propagate cancellation
        }
    }

    /// <summary>
    /// Validates that the process exited successfully
    /// </summary>
    private static void ValidateProcessSuccess(Process process)
    {
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"yt-dlp failed with exit code {process.ExitCode}");
        }
    }

    /// <summary>
    /// Handles output from yt-dlp and extracts progress information
    /// </summary>
    private static void HandleOutput(string? data, IProgress<double>? progress)
    {
        if (string.IsNullOrWhiteSpace(data) || progress == null)
        {
            return;
        }

        var progressValue = ExtractProgress(data);
        if (progressValue.HasValue)
        {
            progress.Report(progressValue.Value);
        }
    }

    /// <summary>
    /// Extracts progress percentage from yt-dlp output
    /// </summary>
    private static double? ExtractProgress(string output)
    {
        // yt-dlp outputs progress like: [download]  45.2% of 3.5MiB at 1.2MiB/s ETA 00:02
        var match = ProgressRegex().Match(output);

        if (match.Success && double.TryParse(match.Groups[1].Value, out var percentage))
        {
            return percentage;
        }

        return null;
    }

    /// <summary>
    /// Normalizes whitespace in filenames (replaces multiple spaces with single space)
    /// </summary>
    private static string NormalizeWhitespace(string filename)
    {
        var normalized = Regex.Replace(filename, @"\s+", " ");
        return normalized.Trim();
    }

    /// <summary>
    /// Finds downloaded files in temp directory, normalizes filenames, and moves to final location
    /// Returns both the audio file path and thumbnail path (if available)
    /// </summary>
    private DownloadResult FindDownloadedFilesAndMove()
    {
        // Find and normalize MP3 file
        var tempMp3File = Directory.GetFiles(_tempDirectory, "*.mp3")
            .OrderByDescending(File.GetCreationTime)
            .FirstOrDefault() ?? throw new FileNotFoundException("Downloaded MP3 file not found in temp directory", _tempDirectory);

        var normalizedMp3Name = NormalizeWhitespace(Path.GetFileName(tempMp3File));
        var normalizedMp3Path = Path.Combine(_tempDirectory, normalizedMp3Name);

        // Rename if needed
        if (tempMp3File != normalizedMp3Path)
        {
            File.Move(tempMp3File, normalizedMp3Path, overwrite: true);
            tempMp3File = normalizedMp3Path;
        }

        // Move MP3 to final download directory with unique name
        var finalAudioPath = Path.Combine(_downloadDirectory, normalizedMp3Name);
        finalAudioPath = GetUniqueFilePath(finalAudioPath);
        File.Move(tempMp3File, finalAudioPath);

        // Find and normalize thumbnail (optional - may not exist)
        string? finalThumbnailPath = null;
        var tempThumbnailFile = Directory.GetFiles(_tempDirectory, "*.jpg")
            .OrderByDescending(File.GetCreationTime)
            .FirstOrDefault();

        if (tempThumbnailFile != null)
        {
            var normalizedThumbnailName = NormalizeWhitespace(Path.GetFileName(tempThumbnailFile));
            var normalizedThumbnailPath = Path.Combine(_tempDirectory, normalizedThumbnailName);

            // Rename if needed
            if (tempThumbnailFile != normalizedThumbnailPath)
            {
                File.Move(tempThumbnailFile, normalizedThumbnailPath, overwrite: true);
                tempThumbnailFile = normalizedThumbnailPath;
            }

            // Convert .jpg to .jpeg for Telegram compatibility
            var jpegThumbnailPath = Path.ChangeExtension(tempThumbnailFile, ".jpeg");
            File.Move(tempThumbnailFile, jpegThumbnailPath, overwrite: true);

            // Process thumbnail for Telegram (crop to square, resize to 320x320 max)
            ProcessThumbnailForTelegram(jpegThumbnailPath);

            // Keep thumbnail in temp directory for Telegram to use
            // It will be cleaned up after sending
            finalThumbnailPath = jpegThumbnailPath;
        }

        return new DownloadResult
        {
            AudioFilePath = finalAudioPath,
            ThumbnailPath = finalThumbnailPath
        };
    }

    /// <summary>
    /// Gets a unique file path by adding (1), (2), etc. if file already exists
    /// </summary>
    private static string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return filePath;
        }

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        var counter = 1;
        string uniquePath;

        do
        {
            var newFileName = $"{fileNameWithoutExt} ({counter}){extension}";
            uniquePath = Path.Combine(directory, newFileName);
            counter++;
        } while (File.Exists(uniquePath));

        return uniquePath;
    }

    /// <summary>
    /// Cleans up temporary files (thumbnails, metadata files) after processing
    /// Keeps only the final MP3 file in download directory
    /// </summary>
    private void CleanupTempFiles(string downloadedFilePath)
    {
        try
        {
            // Get the base name from the actual downloaded file (not the template)
            Console.WriteLine("before baseName");
            var baseName = Path.GetFileNameWithoutExtension(downloadedFilePath);

            Console.WriteLine("baseName: " + baseName);

            // Delete all remaining files in temp directory that match this download
            // This includes .jpg thumbnails, .webp images, metadata files, etc.
            var tempFiles = Directory.GetFiles(_tempDirectory, "*.*")
                .Where(f =>
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(f);
                    return fileNameWithoutExt.Equals(baseName, StringComparison.OrdinalIgnoreCase);
                });

            Console.WriteLine("tempFiles: " + string.Join(", ", tempFiles));

            foreach (var tempFile in tempFiles)
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch
                {
                    // Ignore individual file deletion errors
                }
            }
        }
        catch
        {
            // Ignore cleanup errors - not critical to operation
        }
    }

    /// <summary>
    /// Processes thumbnail to meet Telegram requirements: 320x320 max, JPEG format
    /// Crops image to square and resizes if dimensions exceed 320 pixels
    /// </summary>
    private static void ProcessThumbnailForTelegram(string thumbnailPath)
    {
        try
        {
            using var image = Image.Load(thumbnailPath);

            // Check if processing is needed
            var maxDimension = Math.Max(image.Width, image.Height);
            if (maxDimension <= 320 && image.Width == image.Height)
            {
                // Already 320x320 or smaller and square, no processing needed
                return;
            }

            // Crop to square (center crop)
            var minDimension = Math.Min(image.Width, image.Height);
            var cropX = (image.Width - minDimension) / 2;
            var cropY = (image.Height - minDimension) / 2;

            image.Mutate(x => x
                .Crop(new Rectangle(cropX, cropY, minDimension, minDimension))
                .Resize(new ResizeOptions
                {
                    Size = new Size(320, 320),
                    Mode = ResizeMode.Max // Only resize if larger than 320
                }));

            // Save back to the same file as JPEG
            image.SaveAsJpeg(thumbnailPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process thumbnail: {ex.Message}");
            // Non-critical error, continue without processed thumbnail
        }
    }

    /// <summary>
    /// Regex pattern to extract progress percentage from yt-dlp output
    /// </summary>
    [GeneratedRegex(@"\[download\]\s+(\d+\.?\d*)%")]
    private static partial Regex ProgressRegex();
}
