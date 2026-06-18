using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Persistence.Json;

namespace QuikytLoader.Infrastructure.Youtube;

internal partial class YtDlpService : IYtDlpService
{
    public async Task<Result<VideoMetadata>> GetVideoMetadataAsync(
        DownloadSource downloadSource,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                ArgumentList = { "--quiet", "--skip-download", "--no-playlist", "--print", "id", "--print", "title", "--print", "channel", "--print", "duration", "--print", "thumbnail", "--print", "availability", "--", downloadSource.YoutubeVideoId },
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null) return Errors.Youtube.YtDlpStartFailed();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await WaitForProcessExit(process, cancellationToken);

            if (process.ExitCode != 0)
                return Errors.Youtube.MetadataFetchFailed(downloadSource.YoutubeVideoId);

            var output = await outputTask;
            var lines = output.Split('\n');

            if (lines.Length < 6)
                return Errors.Youtube.MetadataFetchFailed(downloadSource.YoutubeVideoId);

            var (isAvailable, unavailableReason) = DetermineAvailability(lines[5].Trim());

            var metadata = new VideoMetadata(
                VideoId: lines[0].Trim(),
                Title: lines[1].Trim(),
                Channel: lines[2].Trim(),
                DurationInSeconds: TimeSpan.FromSeconds(double.Parse(lines[3].Trim())),
                ThumbnailUrl: lines[4].Trim(),
                IsAvailable: isAvailable,
                UnavailableReason: unavailableReason
            );

            return Result<VideoMetadata>.Success(metadata);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.Youtube.YtDlpException(downloadSource.YoutubeVideoId, ex.GetType().Name);
        }
    }

    public async Task<Result<PlaylistMetadataDto>> GetPlaylistMetadataAsync(
        DownloadPlaylistSource downloadPlaylistSource,
        uint maxItems,
        CancellationToken cancellationToken = default)
    {
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
                    "--", downloadPlaylistSource.YoutubePlaylistUrl
                },
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null) return Errors.Youtube.YtDlpStartFailed();

            var playlistOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await WaitForProcessExit(process, cancellationToken);

            if (process.ExitCode != 0)
                return Errors.Youtube.PlaylistMetadataFetchFailed(downloadPlaylistSource.YoutubePlaylistUrl);

            var playlistOutput = await playlistOutputTask;
            var parsedPlaylist = JsonSerializer.Deserialize(playlistOutput, AppJsonSerializerContext.Default.YtDlpPlaylistJson)!;

            return new PlaylistMetadataDto(
                PlaylistId: parsedPlaylist.Id,
                PlaylistTitle: parsedPlaylist.Title,
                PlaylistVideos: parsedPlaylist.Entries
                    .Select(entry =>
                    {
                        var (isAvailable, unavailableReason) = DetermineAvailability(entry.Availability);
                        return new PlaylistVideoDto(
                            new DownloadSource(entry.Url, entry.Id),
                            new VideoMetadata(
                                entry.Id,
                                entry.Title,
                                entry.Channel,
                                TimeSpan.FromSeconds(entry.Duration),
                                entry.Thumbnails.Last().Url,
                                isAvailable,
                                unavailableReason));
                    }).ToList());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.Youtube.YtDlpException(downloadPlaylistSource.YoutubePlaylistId, ex.GetType().Name);
        }
    }

    private static (bool isAvailable, string unavailableReason) DetermineAvailability(string? availability) =>
        availability switch
        {
            null or "" or "public" or "unlisted" => (true, string.Empty),
            "private" => (false, "Private video"),
            "premium_only" => (false, "Premium only"),
            "subscriber_only" => (false, "Members only"),
            "needs_auth" => (false, "Sign-in required"),
            _ => (false, availability ?? "Unknown")
        };

    public async Task<Result> DownloadAudioAsync(
        DownloadSource downloadSource,
        string downloadDirectory,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = BuildAudioDownloadArguments(downloadSource.YoutubeVideoId, downloadDirectory, customTitle),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return Errors.Youtube.YtDlpStartFailed();

            process.OutputDataReceived += (sender, e) => HandleOutput(e.Data, progress);
            process.ErrorDataReceived += (sender, e) => HandleOutput(e.Data, progress);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await WaitForProcessExit(process, cancellationToken);
            return process.ExitCode == 0
                ? Result.Success()
                : Errors.Youtube.DownloadFailed(downloadSource.YoutubeVideoId, process.ExitCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.Youtube.YtDlpException(downloadSource.YoutubeVideoId, ex.GetType().Name);
        }
    }

    private static string BuildAudioDownloadArguments(string youtubeVideoId, string tempDirectory, string? customTitle)
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
               $"\"{youtubeVideoId}\"";
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
