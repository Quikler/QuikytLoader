using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Infrastructure.YouTube;

internal partial class YoutubeDownloadService(IYoutubeExtractorService youtubeExtractorService, IYtDlpService ytDlpService, IThumbnailService thumbnailService) : IYoutubeDownloadService
{
    private readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format
    /// Files are kept in temp directory for sending to Telegram
    /// </summary>
    public async Task<Result<DownloadResultEntity>> DownloadAudioAsync(string url, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_tempDownloadDirectory))
            Directory.CreateDirectory(_tempDownloadDirectory);

        var youtubeIdResult = await youtubeExtractorService.GetVideoIdAsync(url, cancellationToken);
        if (!youtubeIdResult.IsSuccess)
            return Result<DownloadResultEntity>.Failure(youtubeIdResult.Error);

        var youtubeId = youtubeIdResult.Value;

        var runResult = await ytDlpService.DownloadAudioAsync(url, _tempDownloadDirectory, customTitle: customTitle, progress: progress, cancellationToken: cancellationToken);
        if (!runResult.IsSuccess)
            return Result<DownloadResultEntity>.Failure(runResult.Error);

        var findResult = FindDownloadedFiles(youtubeId);
        if (!findResult.IsSuccess)
            return Result<DownloadResultEntity>.Failure(findResult.Error);

        var result = findResult.Value;
        Console.WriteLine($"Downloaded: {result.TempMediaFilePath}, Thumbnail: {result.TempThumbnailPath ?? "none"}");
        return result;
    }

    private static string NormalizeWhitespace(string filename)
        => string.Join(" ", filename.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    /// Finds downloaded files in temp directory and normalizes filenames
    /// Files remain in temp directory for sending to Telegram
    /// </summary>
    private Result<DownloadResultEntity> FindDownloadedFiles(YouTubeId youtubeId)
    {
        var files = Directory.EnumerateFiles(_tempDownloadDirectory)
            .Where(f => f.EndsWith(".mp3") || f.EndsWith(".jpg"))
            .OrderByDescending(File.GetCreationTime)
            .ToList();

        var tempMp3File = files.Find(f => f.EndsWith(".mp3"));
        if (tempMp3File is null) return Errors.YouTube.FileNotFound(_tempDownloadDirectory);

        var tempThumbnailFile = files.Find(f => f.EndsWith(".jpg"));
        if (tempThumbnailFile is null) return Errors.Thumbnail.FileNotFound(_tempDownloadDirectory);

        var normalizedMp3Path = Path.Combine(_tempDownloadDirectory, NormalizeWhitespace(Path.GetFileName(tempMp3File)));
        File.Move(tempMp3File, normalizedMp3Path, overwrite: true);

        // Normalize whitespace and convert to .jpeg for Telegram compatibility
        var normalizedThumbnailPath = Path.Combine(_tempDownloadDirectory, $"{NormalizeWhitespace(Path.GetFileNameWithoutExtension(tempThumbnailFile))}.jpeg");
        File.Move(tempThumbnailFile, normalizedThumbnailPath, overwrite: true);

        var processResult = thumbnailService.ProcessForTelegram(normalizedThumbnailPath);
        if (!processResult.IsSuccess)
            return Result<DownloadResultEntity>.Failure(processResult.Error);

        return new DownloadResultEntity(
            youtubeId,
            Path.GetFileNameWithoutExtension(normalizedMp3Path),
            normalizedMp3Path,
            normalizedThumbnailPath);
    }
}
