using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuikytLoader.Services;

/// <summary>
/// Service for downloading videos from YouTube using yt-dlp
/// </summary>
public partial class YouTubeDownloadService : IYouTubeDownloadService
{
    private readonly string _downloadDirectory;

    public YouTubeDownloadService()
    {
        // Use ~/Downloads/QuikytLoader as default download directory
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _downloadDirectory = Path.Combine(homeDir, "Downloads", "QuikytLoader");

        EnsureDownloadDirectoryExists();
    }

    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format
    /// </summary>
    public async Task<string> DownloadAsync(string url, IProgress<double>? progress = null)
    {
        ValidateUrl(url);

        var outputPath = GenerateOutputPath();
        var arguments = BuildYtDlpArguments(url, outputPath);

        await RunYtDlpAsync(arguments, progress);

        return FindDownloadedFile(outputPath);
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
    /// Generates the output file path template for yt-dlp
    /// Uses %(title)s to get the actual video title
    /// </summary>
    private string GenerateOutputPath()
    {
        // yt-dlp will automatically sanitize the title for filesystem
        return Path.Combine(_downloadDirectory, "%(title)s");
    }

    /// <summary>
    /// Builds command-line arguments for yt-dlp
    /// </summary>
    private static string BuildYtDlpArguments(string url, string outputPath)
    {
        return $"--extract-audio --audio-format mp3 --audio-quality 0 " +
               $"--output \"{outputPath}.%(ext)s\" " +
               $"--no-playlist " +
               $"--progress " +
               $"\"{url}\"";
    }

    /// <summary>
    /// Runs yt-dlp process and monitors progress
    /// </summary>
    private static async Task RunYtDlpAsync(string arguments, IProgress<double>? progress)
    {
        var processInfo = CreateProcessInfo(arguments);

        using var process = new Process { StartInfo = processInfo };

        process.OutputDataReceived += (sender, e) => HandleOutput(e.Data, progress);
        process.ErrorDataReceived += (sender, e) => HandleOutput(e.Data, progress);

        StartProcess(process);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await WaitForProcessExit(process);

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
    /// </summary>
    private static async Task WaitForProcessExit(Process process)
    {
        await process.WaitForExitAsync();
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
    /// Finds the downloaded MP3 file in the download directory
    /// Since we use %(title)s template, we find the most recently created MP3
    /// </summary>
    private string FindDownloadedFile(string outputPath)
    {
        // Get the most recently created MP3 file in the download directory
        var mp3File = Directory.GetFiles(_downloadDirectory, "*.mp3")
            .OrderByDescending(f => File.GetCreationTime(f))
            .FirstOrDefault();

        if (mp3File != null)
        {
            return mp3File;
        }

        throw new FileNotFoundException("Downloaded MP3 file not found in download directory", _downloadDirectory);
    }

    /// <summary>
    /// Regex pattern to extract progress percentage from yt-dlp output
    /// </summary>
    [GeneratedRegex(@"\[download\]\s+(\d+\.?\d*)%")]
    private static partial Regex ProgressRegex();
}
