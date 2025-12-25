using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Download YouTube video, save to history, send to Telegram
/// </summary>
public class DownloadAndSendUseCase(
    IYoutubeDownloadService youtubeDownloadService,
    IDownloadHistoryRepository historyRepo,
    ITelegramBotService telegramService,
    IYoutubeExtractorService youtubeExtractorService)
{
    public async Task<Result<DownloadResultDto>> ExecuteAsync(
        string url,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Extract YouTube ID
        var youtubeIdResult = await youtubeExtractorService.GetVideoIdAsync(url, cancellationToken);
        if (!youtubeIdResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(youtubeIdResult.Error);

        // 2. Download video
        var downloadResult = await youtubeDownloadService.DownloadAudioAsync(url, customTitle, progress, cancellationToken);

        if (!downloadResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(downloadResult.Error);

        var entity = downloadResult.Value;

        // 3. Send to Telegram
        var sendResult = await telegramService.SendAudioAsync(
            entity.TempMediaFilePath,
            entity.TempThumbnailPath);

        if (!sendResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(sendResult.Error);

        // 4. Save to history
        await historyRepo.UpsertAsync(
            new DownloadHistoryEntity(
                entity.YouTubeId,
                customTitle ?? entity.VideoTitle,
                DateTime.UtcNow.ToString("o")),
            cancellationToken);

        // 5. Map domain entity to DTO and return
        var dto = new DownloadResultDto(entity.YouTubeId.Id, entity.VideoTitle, entity.TempMediaFilePath, entity.TempThumbnailPath);
        return Result<DownloadResultDto>.Success(dto);
    }
}
