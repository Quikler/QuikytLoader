using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QuikytLoader.Services;

/// <summary>
/// Service for extracting YouTube video IDs from URLs.
/// Uses regex for fast extraction, with yt-dlp fallback for edge cases.
/// </summary>
public partial class YoutubeExtractor : IYoutubeExtractor
{
    // Regex patterns for common YouTube URL formats
    // Matches: youtube.com/watch?v=ID, youtu.be/ID, youtube.com/embed/ID, etc.
    [GeneratedRegex(@"(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YoutubeIdRegex();

    public async Task<string?> ExtractIdAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        // Fast path: Try regex extraction first
        var match = YoutubeIdRegex().Match(url);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }

        // Fallback: Use yt-dlp for edge cases
        return await ExtractIdUsingYtDlpAsync(url, cancellationToken);
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
