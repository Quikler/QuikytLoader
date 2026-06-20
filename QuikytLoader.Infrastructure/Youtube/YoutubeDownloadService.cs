using System.Text.RegularExpressions;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Youtube.ACL.Services;

namespace QuikytLoader.Infrastructure.Youtube;

internal partial class YoutubeDownloadService(IYtDlpAcl ytDlpAcl, IThumbnailService thumbnailService) : IYoutubeDownloadService
{
    private static readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    public async Task<Result<DownloadResultEntity>> DownloadAudioAsync(
        DownloadSource downloadSource,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        var downloadDirectory = Path.Combine(_tempDownloadDirectory, downloadSource.YoutubeVideoId);
        Directory.CreateDirectory(downloadDirectory);

        var downloadAudioResult = await ytDlpAcl.DownloadAudioAsync(
            downloadSource,
            downloadDirectory,
            SanitizeFileName(customTitle),
            onOutputLine: line =>
            {
                var p = ExtractProgress(line);
                if (p.HasValue)
                    progress?.Report(p.Value);
            },
            onErrorLine: line =>
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

    private static string? SanitizeFileName(string? customTitle) =>
        string.IsNullOrWhiteSpace(customTitle)
            ? null
            : string.Join(
                "_",
                customTitle.Split(
                    Path.GetInvalidFileNameChars(),
                    StringSplitOptions.RemoveEmptyEntries))
                .Trim();

    private static double? ExtractProgress(string output)
    {
        // yt-dlp outputs progress like: [download]  45.2% of 3.5MiB at 1.2MiB/s ETA 00:02
        var match = ProgressRegex().Match(output);

        if (match.Success && double.TryParse(match.Groups[1].Value, out var percentage))
            return percentage;

        return null;
    }

    private static string NormalizeWhitespace(string filename)
        => string.Join(" ", filename.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

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

        var normalizedMp3Path = Path.Combine(downloadDirectory, NormalizeWhitespace(Path.GetFileName(tempMp3File)));
        File.Move(tempMp3File, normalizedMp3Path, overwrite: true);

        // Normalize whitespace and convert to .jpeg for Telegram compatibility
        var normalizedThumbnailPath = Path.Combine(downloadDirectory, $"{NormalizeWhitespace(Path.GetFileNameWithoutExtension(tempThumbnailFile))}.jpeg");
        File.Move(tempThumbnailFile, normalizedThumbnailPath, overwrite: true);

        var processResult = thumbnailService.ProcessForTelegram(normalizedThumbnailPath);
        if (!processResult.IsSuccess)
            return Result<DownloadResultEntity>.Failure(processResult.Error);

        return new DownloadResultEntity(
            youtubeVideoId,
            Path.GetFileNameWithoutExtension(normalizedMp3Path),
            normalizedMp3Path,
            normalizedThumbnailPath,
            downloadDirectory);
    }

    [GeneratedRegex(@"\[download\]\s+(\d+\.?\d*)%")]
    private static partial Regex ProgressRegex();
}
