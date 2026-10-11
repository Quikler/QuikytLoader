using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

public interface IDownloadAndSendUseCase
{
    Task<Result> ExecuteAsync(
        DownloadSource downloadSource,
        string? customTitle,
        IProgress<double> progress,
        CancellationToken ct = default);
}

public class DownloadAndSendUseCase(
    IYoutubeDownloadService youtubeDownloadService,
    ITempDirectoryService tempDirectoryService,
    IDownloadHistoryRepository historyRepo,
    ITelegramBotService telegramService,
    ITelegramMediaService telegramMediaService)
        : IDownloadAndSendUseCase
{
    public async Task<Result> ExecuteAsync(
        DownloadSource downloadSource,
        string? customTitle,
        IProgress<double> progress,
        CancellationToken ct = default)
    {
        var mediaDirectory =
            tempDirectoryService.CreateSubdirectory(downloadSource.YoutubeVideoId, "media");

        var sanitizedCustomTitle = SanitizeAndCollapseWhitespaces(customTitle);

        // 1. Download video
        var downloadResult = await youtubeDownloadService.DownloadAudioAsync(
            mediaDirectory,
            downloadSource,
            sanitizedCustomTitle,
            progress,
            ct);
        if (!downloadResult.IsSuccess)
            return downloadResult.Error;

        var downloadResultEntity = downloadResult.Value;
        Console.WriteLine($"Downloaded: '{downloadResultEntity.TempMp3FilePath}', Thumbnail: '{downloadResultEntity.TempThumbnailFilePath}'");

        // 2. Send to Telegram
        PreparedMedia preparedMedia;
        if (string.IsNullOrWhiteSpace(sanitizedCustomTitle))
        {
            // No custom title - no rollback
            preparedMedia = telegramMediaService.PrepareMedia(
                mediaDirectory,
                downloadResultEntity.TempMp3FilePath,
                downloadResultEntity.TempThumbnailFilePath);

            var sendResult = await telegramService.SendAudioAsync(
                preparedMedia.FinalMp3Path,
                preparedMedia.FinalThumbnailPath);

            if (!sendResult.IsSuccess)
                return sendResult.Error;
        }
        else
        {
            // Custom title - rollback on failure
            preparedMedia = telegramMediaService.PrepareMediaWithTitle(
                mediaDirectory,
                downloadResultEntity.TempMp3FilePath,
                downloadResultEntity.TempThumbnailFilePath,
                sanitizedCustomTitle);

            try
            {
                var sendResult = await telegramService.SendAudioAsync(
                    preparedMedia.FinalMp3Path,
                    preparedMedia.FinalThumbnailPath);

                if (!sendResult.IsSuccess)
                {
                    telegramMediaService.Rollback(preparedMedia);
                    return sendResult.Error;
                }
            }
            catch (OperationCanceledException)
            {
                telegramMediaService.Rollback(preparedMedia);
                throw;
            }
        }
        Console.WriteLine($"Audio file sent to Telegram: '{Path.GetFileName(preparedMedia.FinalMp3Path)}' with thumbnail '{Path.GetFileName(preparedMedia.FinalThumbnailPath)}'");

        // 3. Save to history
        await historyRepo.UpsertAsync(
            new DownloadHistoryEntity(
                downloadResultEntity.YoutubeVideoId,
                Path.GetFileNameWithoutExtension(preparedMedia.FinalMp3Path),
                DateTime.UtcNow.ToString("o")));

        // 4. Delete created media temporary directory only if everything succeeded
        tempDirectoryService.DeleteSubdirectory(mediaDirectory);

        return Result.Success();
    }

    private static string? SanitizeAndCollapseWhitespaces(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        // Remove invalid filename chars
        var sanitized = string.Join(
            " ",
            fileName.Split(
                Path.GetInvalidFileNameChars(),
                StringSplitOptions.RemoveEmptyEntries));

        // Replace whitespaces with one space "foo    bar" -> "foo bar"
        return string.Join(
            " ",
            sanitized.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries));
    }
}
