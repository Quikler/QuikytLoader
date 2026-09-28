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
    ITelegramBotService telegramService)
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
        // ========= SETUP =========
        var sourceMp3 = downloadResultEntity.TempMp3FilePath; // media/title.mp3
        var sourceThumbnail = downloadResultEntity.TempThumbnailFilePath; // media/title.jpg
        // Convert to .jpeg for Telegram compatibility
        var normalizedThumbnailPath = Path.Combine(mediaDirectory, $"{Path.GetFileNameWithoutExtension(sourceThumbnail)}.jpeg");
        File.Move(sourceThumbnail, normalizedThumbnailPath, overwrite: true);
        sourceThumbnail = normalizedThumbnailPath;

        // =========================
        if (!string.IsNullOrWhiteSpace(sanitizedCustomTitle))
        {
            {
                var destMp3 = Path.Combine(mediaDirectory, sanitizedCustomTitle + Path.GetExtension(sourceMp3));
                File.Move(sourceMp3, destMp3);
                sourceMp3 = destMp3; // media/title.mp3 -> media/customTitle.mp3

                var destThumbnail = Path.Combine(mediaDirectory, sanitizedCustomTitle + Path.GetExtension(sourceThumbnail));
                File.Move(sourceThumbnail, destThumbnail);
                sourceThumbnail = destThumbnail; // media/title.jpeg -> media/customTitle.jpeg
            }

            try
            {
                var sendResult = await telegramService.SendAudioAsync(
                    sourceMp3,
                    sourceThumbnail);
                if (!sendResult.IsSuccess) // Rollback
                {
                    File.Move(sourceMp3, downloadResultEntity.TempMp3FilePath);
                    File.Move(sourceThumbnail, normalizedThumbnailPath);
                    return sendResult.Error;
                }
            }
            catch (OperationCanceledException) // Rollback
            {
                File.Move(sourceMp3, downloadResultEntity.TempMp3FilePath);
                File.Move(sourceThumbnail, normalizedThumbnailPath);
                throw;
            }
            // P.S. Rollback is needed so if user wants to retry downloading
            // yt-dlp will identify file by it's original title
        }
        else
        {
            var sendResult = await telegramService.SendAudioAsync(
                sourceMp3,
                sourceThumbnail);
            if (!sendResult.IsSuccess)
                return sendResult.Error;
        }
        Console.WriteLine($"Audio file sent to Telegram: '{Path.GetFileName(sourceMp3)}' with thumbnail '{Path.GetFileName(sourceThumbnail)}'");

        // 3. Save to history
        await historyRepo.UpsertAsync(
            new DownloadHistoryEntity(
                downloadResultEntity.YoutubeVideoId,
                Path.GetFileNameWithoutExtension(sourceMp3),
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
