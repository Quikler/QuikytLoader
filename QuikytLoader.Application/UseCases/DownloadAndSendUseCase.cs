using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Download YouTube video, save to history, send to Telegram
/// Orchestrates multiple services to complete the workflow
/// </summary>
public class DownloadAndSendUseCase(
    IYouTubeDownloadService downloadService,
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
        var youtubeIdResult = await youtubeExtractorService.ExtractVideoIdAsync(url, cancellationToken);
        if (!youtubeIdResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(youtubeIdResult.Error);

        // 2. Download video
        var downloadResult = customTitle != null
            ? await downloadService.DownloadAsync(url, customTitle, progress, cancellationToken)
            : await downloadService.DownloadAsync(url, progress, cancellationToken);

        if (!downloadResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(downloadResult.Error);

        var downloadResultValue = downloadResult.Value;

        // 3. Send to Telegram
        var sendResult = await telegramService.SendAudioAsync(
            downloadResultValue.TempMediaFilePath,
            downloadResultValue.TempThumbnailPath);

        if (!sendResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(sendResult.Error);

        // 4. Save to history (non-critical - don't fail entire operation on error)
        await historyRepo.SaveAsync(
            new DownloadEntity(
                youtubeIdResult.Value,
                customTitle ?? downloadResultValue.VideoTitle,
                DateTime.UtcNow.ToString("o")),
            cancellationToken);

        // 5. Return success with DTO
        return Result<DownloadResultDto>.Success(downloadResultValue);
    }
}
