using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Infrastructure.Persistence.Json;

namespace QuikytLoader.Infrastructure.YouTube;

internal partial class YtDlpService : IYtDlpService
{
    public async Task<Result<string>> GetVideoIdAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                ArgumentList = { "--quiet", "--print", "id", "--skip-download", "--", url },
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return Errors.YouTube.YtDlpStartFailed();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await WaitForProcessExit(process, cancellationToken);

            if (process.ExitCode != 0)
                return Errors.YouTube.YtDlpExtractionFailed(url, process.ExitCode);

            var output = await outputTask;
            var id = output.Trim();

            return id.Length == 11
                ? Result<string>.Success(id)
                : Errors.YouTube.InvalidIdLength(url, id, id.Length);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.YouTube.YtDlpException(url, ex.GetType().Name);
        }
    }

    public async Task<Result<string>> GetVideoTitleAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Errors.YouTube.InvalidUrl(url);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                ArgumentList = { "--quiet", "--get-title", "--no-playlist", "--", url },
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null) return Errors.YouTube.YtDlpStartFailed();

            var outputBuilder = new System.Text.StringBuilder();
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.BeginOutputReadLine();

            await WaitForProcessExit(process, cancellationToken);

            if (process.ExitCode != 0)
                return Errors.YouTube.DownloadFailed(url, process.ExitCode);

            var title = outputBuilder.ToString().Trim();
            if (string.IsNullOrWhiteSpace(title))
                return Errors.YouTube.TitleFetchFailed(url);

            return Result<string>.Success(title);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.YouTube.YtDlpException(url, ex.GetType().Name);
        }
    }

    public async Task<Result<VideoMetadataDto>> GetVideoMetadataAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Errors.YouTube.InvalidUrl(url);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                ArgumentList = { "--quiet", "--skip-download", "--no-playlist", "--print", "id", "--print", "title", "--print", "channel", "--print", "duration_string", "--print", "thumbnail", "--print", "availability", "--", url },
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null) return Errors.YouTube.YtDlpStartFailed();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await WaitForProcessExit(process, cancellationToken);

            if (process.ExitCode != 0)
                return Errors.YouTube.MetadataFetchFailed(url);

            var output = await outputTask;
            var lines = output.Split('\n');

            if (lines.Length < 4)
                return Errors.YouTube.MetadataFetchFailed(url);

            var (isAvailable, unavailableReason) = DetermineAvailability(lines[5].Trim());

            var metadata = new VideoMetadataDto(
                VideoId: lines[0].Trim(),
                Title: lines[1].Trim(),
                Channel: lines[2].Trim(),
                Duration: lines[3].Trim(),
                ThumbnailUrl: lines[4].Trim(),
                IsAvailable: isAvailable,
                UnavailableReason: unavailableReason
            );

            return Result<VideoMetadataDto>.Success(metadata);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.YouTube.YtDlpException(url, ex.GetType().Name);
        }
    }

    public async Task<Result<PlaylistMetadataDto>> GetPlaylistMetadataAsync(string url, int maxItems, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url) || maxItems <= 0)
            return Errors.YouTube.InvalidPlaylistUrl(url);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                ArgumentList =
                {
                    "--quiet",
                    "--flat-playlist",
                    "--playlist-items", $"1:{maxItems}",
                    "--dump-single-json",
                    "--", url
                },
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null) return Errors.YouTube.YtDlpStartFailed();

            var playlistOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await WaitForProcessExit(process, cancellationToken);

            if (process.ExitCode != 0)
                return Errors.YouTube.PlaylistFetchFailed(url);

            var playlistOutput = await playlistOutputTask;
            var parsedPlaylist = JsonSerializer.Deserialize(playlistOutput, AppJsonSerializerContext.Default.YtDlpPlaylistJson)!;

            return new PlaylistMetadataDto(
                PlaylistId: parsedPlaylist.Id,
                Title: parsedPlaylist.Title,
                PlaylistVideos: parsedPlaylist.Entries.Select(BuildEntryDto).ToList());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.YouTube.YtDlpException(url, ex.GetType().Name);
        }
    }

    private static VideoMetadataDto BuildEntryDto(YtDlpPlaylistEntryJson entry)
    {
        var (isAvailable, unavailableReason) = DetermineAvailability(entry.Availability);
        return new VideoMetadataDto(entry.Id, entry.Title, entry.Channel, FormatDuration(entry.Duration), entry.Thumbnails.LastOrDefault()?.Url!, isAvailable, unavailableReason);
    }

    private static string FormatDuration(double totalSeconds)
    {
        var ts = TimeSpan.FromSeconds(totalSeconds);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes}:{ts.Seconds:D2}";
    }

    private static (bool isAvailable, string unavailableReason) DetermineAvailability(string? availability) =>
        availability switch
        {
            null or "" or "public" or "unlisted" => (true, string.Empty),
            "private" => (false, "Private video"),
            "premium_only" => (false, "Premium only"),
            "subscriber_only" => (false, "Members only"),
            "needs_auth" => (false, "Sign-in required"),
            _ => (false, availability ?? "Unkown")
        };

    // TODO: we are specifying "tempDirectory", so makes sense to return the download location info in return type
    public async Task<Result> DownloadAudioAsync(string url, string tempDirectory, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = BuildAudioDownloadArguments(url, tempDirectory, customTitle),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return Errors.YouTube.YtDlpStartFailed();

            process.OutputDataReceived += (sender, e) => HandleOutput(e.Data, progress);
            process.ErrorDataReceived += (sender, e) => HandleOutput(e.Data, progress);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await WaitForProcessExit(process, cancellationToken);
            return process.ExitCode == 0
                ? Result.Success()
                : Errors.YouTube.DownloadFailed(url, process.ExitCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.YouTube.YtDlpException(url, ex.GetType().Name);
        }
    }

    private static string BuildAudioDownloadArguments(string url, string tempDirectory, string? customTitle)
    {
        var sanitizedTitle = !string.IsNullOrWhiteSpace(customTitle)
            ? string.Join("_", customTitle.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim()
            : "%(title)s";

        var outputPath = Path.Combine(tempDirectory, sanitizedTitle);

        return $"--extract-audio " +
               $"--audio-format mp3 " +
               $"--audio-quality 0 " +
               $"--output \"{outputPath}.%(ext)s\" " +
               $"--no-playlist " +
               $"--add-metadata " +
               $"--embed-thumbnail " +
               $"--write-thumbnail " +
               $"--convert-thumbnails jpg " +
               $"--parse-metadata \"{sanitizedTitle}:%(meta_title)s\" " +
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
               $"--progress " +
               $"\"{url}\"";
    }

    private static void HandleOutput(string? data, IProgress<double>? progress)
    {
        if (string.IsNullOrWhiteSpace(data) || progress is null) return;

        var progressValue = ExtractProgress(data);
        if (progressValue.HasValue)
            progress.Report(progressValue.Value);
    }

    private static double? ExtractProgress(string output)
    {
        // yt-dlp outputs progress like: [download]  45.2% of 3.5MiB at 1.2MiB/s ETA 00:02
        var match = ProgressRegex().Match(output);

        if (match.Success && double.TryParse(match.Groups[1].Value, out var percentage))
            return percentage;

        return null;
    }

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
                await process.WaitForExitAsync(CancellationToken.None); // Wait for the process to fully exit (don't use cancelled token)
            }
            throw; // Re-throw to propagate cancellation
        }
    }

    [GeneratedRegex(@"\[download\]\s+(\d+\.?\d*)%")]
    private static partial Regex ProgressRegex();
}
