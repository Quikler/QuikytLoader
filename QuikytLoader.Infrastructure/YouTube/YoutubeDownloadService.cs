using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Infrastructure.YouTube;

internal partial class YoutubeDownloadService(IYoutubeExtractorService youtubeExtractorService, IYtDlpService ytDlpService, IThumbnailService thumbnailService) : IYoutubeDownloadService
{
    private readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format
    /// Files are kept in temp directory for sending to Telegram
    /// </summary>
    public async Task<Result<DownloadResultDto>> DownloadAudioAsync(string url, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        EnsureTempDirectoryExists();

        var youtubeIdResult = await youtubeExtractorService.ExtractVideoIdAsync(url, cancellationToken);
        if (!youtubeIdResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(youtubeIdResult.Error);

        var youtubeId = youtubeIdResult.Value;

        var runResult = await ytDlpService.DownloadAudioAsync(url, _tempDownloadDirectory, customTitle: customTitle, progress: progress, cancellationToken: cancellationToken);
        if (!runResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(runResult.Error);

        var findResult = FindDownloadedFiles(youtubeId.Value);
        if (!findResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(findResult.Error);

        var result = findResult.Value;
        Console.WriteLine($"Downloaded: {result.TempMediaFilePath}, Thumbnail: {result.TempThumbnailPath ?? "none"}");

        return Result<DownloadResultDto>.Success(result);
    }

    private static string NormalizeWhitespace(string filename)
        => string.Join(" ", filename.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private void EnsureTempDirectoryExists()
    {
        if (!Directory.Exists(_tempDownloadDirectory))
            Directory.CreateDirectory(_tempDownloadDirectory);
    }

    /// <summary>
    /// Finds downloaded files in temp directory and normalizes filenames
    /// Returns both the audio file path and thumbnail path (if available)
    /// Files remain in temp directory for sending to Telegram
    /// </summary>
    private Result<DownloadResultDto> FindDownloadedFiles(string youtubeId)
    {
        // Find and normalize MP3 file
        var tempMp3File = Directory.GetFiles(_tempDownloadDirectory, "*.mp3")
            .OrderByDescending(File.GetCreationTime)
            .FirstOrDefault();

        if (tempMp3File == null)
            return Errors.YouTube.FileNotFound(_tempDownloadDirectory);

        var normalizedMp3Name = NormalizeWhitespace(Path.GetFileName(tempMp3File));
        var normalizedMp3Path = Path.Combine(_tempDownloadDirectory, normalizedMp3Name);

        // Rename if needed
        if (tempMp3File != normalizedMp3Path)
        {
            File.Move(tempMp3File, normalizedMp3Path, overwrite: true);
            tempMp3File = normalizedMp3Path;
        }

        // Find and normalize thumbnail (optional - may not exist)
        string? tempThumbnailPath = null;
        var tempThumbnailFile = Directory.GetFiles(_tempDownloadDirectory, "*.jpg")
            .OrderByDescending(File.GetCreationTime)
            .FirstOrDefault();

        if (tempThumbnailFile != null)
        {
            var normalizedThumbnailName = NormalizeWhitespace(Path.GetFileName(tempThumbnailFile));
            var normalizedThumbnailPath = Path.Combine(_tempDownloadDirectory, normalizedThumbnailName);

            // Rename if needed
            if (tempThumbnailFile != normalizedThumbnailPath)
            {
                File.Move(tempThumbnailFile, normalizedThumbnailPath, overwrite: true);
                tempThumbnailFile = normalizedThumbnailPath;
            }

            // Convert .jpg to .jpeg for Telegram compatibility
            var jpegThumbnailPath = Path.ChangeExtension(tempThumbnailFile, ".jpeg");
            File.Move(tempThumbnailFile, jpegThumbnailPath, overwrite: true);

            // Process thumbnail for Telegram (crop to square, resize to 320x320 max)
            var processResult = thumbnailService.ProcessForTelegram(jpegThumbnailPath);
            if (!processResult.IsSuccess)
            {
                Console.WriteLine($"Warning: Failed to process thumbnail: {processResult.Error.Message}");
                // Continue without processed thumbnail
            }

            // Keep thumbnail in temp directory for Telegram to use
            // It will be cleaned up after sending
            tempThumbnailPath = jpegThumbnailPath;
        }

        // Extract video title from filename (without extension)
        var videoTitle = Path.GetFileNameWithoutExtension(tempMp3File);

        var downloadResult = new DownloadResultDto
        {
            YouTubeId = youtubeId,
            VideoTitle = videoTitle,
            TempMediaFilePath = tempMp3File,
            TempThumbnailPath = tempThumbnailPath
        };

        return Result<DownloadResultDto>.Success(downloadResult);
    }
}
