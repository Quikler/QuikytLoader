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

        try
        {
            // 1. Download video
            var downloadResult = await youtubeDownloadService.DownloadAudioAsync(
                mediaDirectory,
                downloadSource,
                customTitle,
                progress,
                ct);
            if (!downloadResult.IsSuccess)
                return downloadResult.Error;

            var downloadResultEntity = downloadResult.Value;
            Console.WriteLine($"Downloaded: {downloadResultEntity.TempMp3FilePath}, Thumbnail: {downloadResultEntity.TempThumbnailFilePath}");

            await using var mp3FileStream = File.OpenRead(downloadResultEntity.TempMp3FilePath);
            await using var thumbnailFileStream = File.OpenRead(downloadResultEntity.TempThumbnailFilePath);

            // 2. Send to Telegram
            var sendResult = await telegramService.SendAudioAsync(mp3FileStream, thumbnailFileStream);
            if (!sendResult.IsSuccess)
                return sendResult.Error;

            Console.WriteLine($"Audio file sent to Telegram: {Path.GetFileName(mp3FileStream.Name)}");

            // 3. Save to history
            await historyRepo.UpsertAsync(
                new DownloadHistoryEntity(
                    downloadResultEntity.YoutubeVideoId,
                    customTitle ?? downloadResultEntity.VideoTitle,
                    DateTime.UtcNow.ToString("o")));

            return Result.Success();
        }
        finally
        {
            // 4. Delete created temporary directory that contains files — no longer needed after Telegram send
            tempDirectoryService.DeleteSubdirectory(mediaDirectory);
        }
    }
}
