using System.Diagnostics;
using System.Text.RegularExpressions;
using QuikytLoader.Application.Interfaces.Services;
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

    public async Task<YouTubeId?> ExtractVideoIdAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        // Fast path: Try regex extraction first
        var match = YoutubeIdRegex().Match(url);
        if (match.Success && match.Groups.Count > 1)
        {
            var idString = match.Groups[1].Value;
            try
            {
                return new YouTubeId(idString);
            }
            catch
            {
                // Invalid ID format, fall through to yt-dlp
            }
        }

        // Fallback: Use yt-dlp for edge cases
        var extractedId = await ExtractIdUsingYtDlpAsync(url, cancellationToken);
        if (extractedId != null)
        {
            try
            {
                return new YouTubeId(extractedId);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static async Task<string?> ExtractIdUsingYtDlpAsync(string url, CancellationToken cancellationToken)
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
                return null;

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                return null;

            var output = await outputTask;
            var id = output.Trim();

            // YouTube IDs are always 11 characters
            return id.Length == 11 ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
