using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Download YouTube video, send to Telegram, save to history, cleanup temp files
/// </summary>
public class DownloadAndSendUseCase(
    IYoutubeDownloadService youtubeDownloadService,
    IDownloadHistoryRepository historyRepo,
    ITelegramBotService telegramService,
    IYoutubeExtractorService youtubeExtractorService)
{
    public async Task<Result> ExecuteAsync(
        string url,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Extract YouTube ID
        var youtubeIdResult = await youtubeExtractorService.GetVideoIdAsync(url, cancellationToken);
        if (!youtubeIdResult.IsSuccess)
            return youtubeIdResult.Error;

        // 2. Download video
        var downloadResult = await youtubeDownloadService.DownloadAudioAsync(url, customTitle, progress, cancellationToken);
        if (!downloadResult.IsSuccess)
            return downloadResult.Error;

        var entity = downloadResult.Value;

        try
        {
            // 3. Send to Telegram
            var sendResult = await telegramService.SendAudioAsync(
                entity.TempMediaFilePath,
                entity.TempThumbnailPath);

            if (!sendResult.IsSuccess)
                return sendResult.Error;

            // 4. Save to history
            await historyRepo.UpsertAsync(
                new DownloadHistoryEntity(
                    entity.YouTubeId,
                    customTitle ?? entity.VideoTitle,
                    DateTime.UtcNow.ToString("o")));

            return Result.Success();
        }
        finally
        {
            // 5. Cleanup temp files — no longer needed after Telegram send
            try { File.Delete(entity.TempMediaFilePath); } catch { }
            try { File.Delete(entity.TempThumbnailPath); } catch { }
        }
    }
}
