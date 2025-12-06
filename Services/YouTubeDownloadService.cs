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
/// Downloads media to temp directory for sending to Telegram only (not stored locally)
/// </summary>
public partial class YouTubeDownloadService(IYoutubeExtractor youtubeExtractor) : IYouTubeDownloadService
{
    private readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format
    /// Files are kept in temp directory for sending to Telegram
    /// </summary>
    public async Task<DownloadResult> DownloadAsync(string url, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        EnsureTempDirectoryExists();

        ValidateUrl(url);

        // Extract YouTube ID
        var youtubeId = await youtubeExtractor.ExtractIdAsync(url, cancellationToken)
            ?? throw new InvalidOperationException("Failed to extract YouTube video ID from URL");

        var tempOutputPath = GenerateTempOutputPath();
        var arguments = BuildYtDlpArguments(url, tempOutputPath);

        await RunYtDlpAsync(arguments, progress, cancellationToken);

        var result = FindDownloadedFiles(youtubeId);
        Console.WriteLine($"Downloaded: {result.TempMediaFilePath}, Thumbnail: {result.TempThumbnailPath ?? "none"}");

        return result;
    }

    /// <summary>
    /// Downloads a video from YouTube with a custom filename and converts it to MP3 format
    /// Files are kept in temp directory for sending to Telegram
    /// </summary>
    public async Task<DownloadResult> DownloadAsync(string url, string customTitle, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        EnsureTempDirectoryExists();

        ValidateUrl(url);

        // Extract YouTube ID
        var youtubeId = await youtubeExtractor.ExtractIdAsync(url, cancellationToken)
            ?? throw new InvalidOperationException("Failed to extract YouTube video ID from URL");

        var tempOutputPath = GenerateTempOutputPathWithCustomTitle(customTitle);
        Console.WriteLine($"Temp output path: {tempOutputPath}");
        var arguments = BuildYtDlpArgumentsWithCustomTitle(url, tempOutputPath, customTitle);

        await RunYtDlpAsync(arguments, progress, cancellationToken);

        var result = FindDownloadedFiles(youtubeId);
        Console.WriteLine($"Downloaded: {result.TempMediaFilePath}, Thumbnail: {result.TempThumbnailPath ?? "none"}");

        return result;
    }

    /// <summary>
    /// Fetches the video title from YouTube without downloading
    /// </summary>
    public async Task<string> GetVideoTitleAsync(string url)
    {
        ValidateUrl(url);

        var arguments = $"--get-title --no-playlist \"{url}\"";
        var processInfo = CreateProcessInfo(arguments);

        using var process = new Process { StartInfo = processInfo };

        var outputBuilder = new System.Text.StringBuilder();
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        StartProcess(process);
        process.BeginOutputReadLine();

        await process.WaitForExitAsync();

        ValidateProcessSuccess(process);

        var title = outputBuilder.ToString().Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Failed to fetch video title");
        }

        return title;
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
    /// Ensures the temporary download directory exists
    /// </summary>
    private void EnsureTempDirectoryExists()
    {
        if (!Directory.Exists(_tempDownloadDirectory))
        {
            Directory.CreateDirectory(_tempDownloadDirectory);
        }
    }

    /// <summary>
    /// Generates the output file path template for yt-dlp in temp directory
    /// Uses %(title)s to get the actual video title
    /// </summary>
    private string GenerateTempOutputPath()
    {
        // yt-dlp will automatically sanitize the title for filesystem
        return Path.Combine(_tempDownloadDirectory, "%(title)s");
    }

    /// <summary>
    /// Generates the output file path with a custom title for yt-dlp in temp directory
    /// Uses custom title instead of video title
    /// </summary>
    private string GenerateTempOutputPathWithCustomTitle(string customTitle)
    {
        // Sanitize the custom title for filesystem use
        var sanitized = SanitizeFilename(customTitle);
        return Path.Combine(_tempDownloadDirectory, sanitized);
    }

    /// <summary>
    /// Sanitizes a filename by removing or replacing invalid characters
    /// </summary>
    private static string SanitizeFilename(string filename)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", filename.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
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
    /// Builds command-line arguments for yt-dlp with custom title
    /// Uses custom title for metadata instead of YouTube video title
    /// Other metadata (artist, album, etc.) still comes from YouTube
    /// </summary>
    private static string BuildYtDlpArgumentsWithCustomTitle(string url, string outputPath, string customTitle)
    {
        return $"--extract-audio " +
               $"--audio-format mp3 " +
               $"--audio-quality 0 " +
               $"--output \"{outputPath}.%(ext)s\" " +
               $"--no-playlist " +
               $"--add-metadata " +
               $"--embed-thumbnail " +
               $"--parse-metadata \"{customTitle}:%(meta_title)s\" " +
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
                await process.WaitForExitAsync(cancellationToken); // Wait for the process to fully exit
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
    /// Finds downloaded files in temp directory and normalizes filenames
    /// Returns both the audio file path and thumbnail path (if available)
    /// Files remain in temp directory for sending to Telegram
    /// </summary>
    private DownloadResult FindDownloadedFiles(string youtubeId)
    {
        // Find and normalize MP3 file
        var tempMp3File = Directory.GetFiles(_tempDownloadDirectory, "*.mp3")
            .OrderByDescending(File.GetCreationTime)
            .FirstOrDefault() ?? throw new FileNotFoundException("Downloaded MP3 file not found in temp directory", _tempDownloadDirectory);

        var normalizedMp3Name = NormalizeWhitespace(Path.GetFileName(tempMp3File));
        var normalizedMp3Path = Path.Combine(_tempDownloadDirectory, normalizedMp3Name);

        // Rename if needed
        if (tempMp3File != normalizedMp3Path)
        {
            File.Move(tempMp3File, normalizedMp3Path, overwrite: true);
            tempMp3File = normalizedMp3Path;
        }

        // Find and normalize thumbnail (optional - may not exist)
        string? tempThumbnailPath = null;
        var tempThumbnailFile = Directory.GetFiles(_tempDownloadDirectory, "*.jpg")
            .OrderByDescending(File.GetCreationTime)
            .FirstOrDefault();

        if (tempThumbnailFile != null)
        {
            var normalizedThumbnailName = NormalizeWhitespace(Path.GetFileName(tempThumbnailFile));
            var normalizedThumbnailPath = Path.Combine(_tempDownloadDirectory, normalizedThumbnailName);

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
            tempThumbnailPath = jpegThumbnailPath;
        }

        return new DownloadResult
        {
            YouTubeId = youtubeId,
            TempMediaFilePath = tempMp3File,
            TempThumbnailPath = tempThumbnailPath
        };
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
