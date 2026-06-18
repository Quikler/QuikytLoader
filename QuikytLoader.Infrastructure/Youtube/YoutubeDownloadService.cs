using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Infrastructure.Youtube;

internal partial class YoutubeDownloadService(IYtDlpService ytDlpService, IThumbnailService thumbnailService) : IYoutubeDownloadService
{
    private static readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    /// <summary>
    /// Downloads a video from Youtube and converts it to MP3 format
    /// Files are kept in temp directory for sending to Telegram
    /// </summary>
    public async Task<Result<DownloadResultEntity>> DownloadAudioAsync(
        DownloadSource downloadSource,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var downloadDirectory = Path.Combine(_tempDownloadDirectory, downloadSource.YoutubeVideoId);
        Directory.CreateDirectory(downloadDirectory);

        var downloadAudioResult = await ytDlpService.DownloadAudioAsync(
            downloadSource,
            downloadDirectory,
            customTitle,
            progress,
            cancellationToken);

        return downloadAudioResult.IsSuccess
            ? FindDownloadedFiles(downloadDirectory, downloadSource.YoutubeVideoId)
            : downloadAudioResult.Error;
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
}
