using System.Text.RegularExpressions;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Youtube.ACL.Services;

namespace QuikytLoader.Infrastructure.Youtube;

internal partial class YoutubeDownloadService(IYtDlpAcl ytDlpAcl) : IYoutubeDownloadService
{
    public async Task<Result<DownloadResultEntity>> DownloadAudioAsync(
        string downloadDirectory,
        DownloadSource downloadSource,
        string? metadataTitle = null,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        var downloadAudioResult = await ytDlpAcl.DownloadAudioAsync(
            downloadSource,
            downloadDirectory,
            metadataTitle,
            onOutputLine: line =>
            {
                var p = ExtractProgress(line);
                if (p.HasValue)
                    progress?.Report(p.Value);
            },
            ct);

        return downloadAudioResult.IsSuccess
            ? FindDownloadedFiles(downloadDirectory, downloadSource.YoutubeVideoId)
            : downloadAudioResult.Error;
    }

    private static double? ExtractProgress(string output)
    {
        // yt-dlp outputs progress like: [download]  45.2% of 3.5MiB at 1.2MiB/s ETA 00:02
        var match = ProgressRegex().Match(output);

        if (match.Success && double.TryParse(match.Groups[1].Value, out var percentage))
            return percentage;

        return null;
    }

    /// <summary>
    /// Finds downloaded files in temp directory and normalizes filenames
    /// Files remain in temp directory for sending to Telegram
    /// </summary>
    private Result<DownloadResultEntity> FindDownloadedFiles(string downloadDirectory, string youtubeVideoId)
    {
        var files = Directory.EnumerateFiles(downloadDirectory)
            .Where(f => f.EndsWith(".mp3") || f.EndsWith(".jpg"))
            .OrderByDescending(File.GetCreationTime)
            .ToList();

        var tempMp3File = files.Find(f => f.EndsWith(".mp3"));
        if (tempMp3File is null) return Errors.Youtube.FileNotFound(downloadDirectory);

        var tempThumbnailFile = files.Find(f => f.EndsWith(".jpg"));
        if (tempThumbnailFile is null) return Errors.Thumbnail.FileNotFound(downloadDirectory);

        return new DownloadResultEntity(
            youtubeVideoId,
            tempMp3File,
            tempThumbnailFile);
    }

    [GeneratedRegex(@"\[download\]\s+(\d+\.?\d*)%")]
    private static partial Regex ProgressRegex();
}
