using System.Diagnostics;
using System.Text.RegularExpressions;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Infrastructure.YouTube;

/// <summary>
/// Service for extracting YouTube video IDs from URLs.
/// Uses regex for fast extraction, with yt-dlp fallback for edge cases.
/// </summary>
internal partial class YoutubeExtractorService : IYoutubeExtractorService
{
    // Regex patterns for common YouTube URL formats
    // Matches: youtube.com/watch?v=ID, youtu.be/ID, youtube.com/embed/ID, etc.
    [GeneratedRegex(@"(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YoutubeIdRegex();

    public async Task<Result<YouTubeId>> ExtractVideoIdAsync(string url, CancellationToken cancellationToken = default)
    {
        // Explicit validation with specific error
        if (string.IsNullOrWhiteSpace(url))
            return Errors.YouTube.InvalidUrl(url);

        // Fast path: Try regex extraction first
        var match = YoutubeIdRegex().Match(url);
        if (match.Success && match.Groups.Count > 1)
        {
            var idString = match.Groups[1].Value;
            var regexIdResult = YouTubeId.Create(idString);
            if (regexIdResult.IsSuccess)
            {
                return regexIdResult;
            }
            // Invalid ID format from regex - fall through to yt-dlp
        }

        // Fallback: Use yt-dlp for edge cases
        var ytDlpResult = await ExtractIdUsingYtDlpAsync(url, cancellationToken);
        if (!ytDlpResult.IsSuccess)
            return Result<YouTubeId>.Failure(ytDlpResult.Error);

        var youtubeIdResult = YouTubeId.Create(ytDlpResult.Value);
        if (!youtubeIdResult.IsSuccess)
        {
            // yt-dlp returned invalid ID format
            return Errors.YouTube.VideoIdExtractionFailed(url);
        }

        return youtubeIdResult;
    }

    private static async Task<Result<string>> ExtractIdUsingYtDlpAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--print id --skip-download \"{url}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return Errors.YouTube.ProcessStartFailed();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                // Specific error with exit code
                return Errors.YouTube.YtDlpExtractionFailed(url, process.ExitCode);
            }

            var output = await outputTask;
            var id = output.Trim();

            // Validate ID length (YouTube IDs are always 11 characters)
            if (id.Length != 11)
            {
                return Errors.YouTube.InvalidIdLength(url, id, id.Length);
            }

            return Result<string>.Success(id);
        }
        catch (OperationCanceledException)
        {
            throw; // Cancellation is exceptional - propagate
        }
        catch (Exception ex)
        {
            // Wrap unexpected errors with context
            return Errors.YouTube.YtDlpException(url, ex.GetType().Name);
        }
    }
}
