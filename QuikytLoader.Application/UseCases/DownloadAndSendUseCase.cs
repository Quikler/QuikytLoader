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
            return Result<DownloadResultDto>.Failure(youtubeIdResult.Error!);

        var youtubeId = youtubeIdResult.Value!;

        // 2. Download video
        var downloadResult = customTitle != null
            ? await downloadService.DownloadAsync(url, customTitle, progress, cancellationToken)
            : await downloadService.DownloadAsync(url, progress, cancellationToken);

        if (!downloadResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(downloadResult.Error!);

        var result = downloadResult.Value!;

        // 3. Send to Telegram
        var sendResult = await telegramService.SendAudioAsync(
            result.TempMediaFilePath,
            result.TempThumbnailPath);

        if (!sendResult.IsSuccess)
            return Result<DownloadResultDto>.Failure(sendResult.Error!);

        // 4. Save to history (non-critical - don't fail entire operation on error)
        try
        {
            var record = new DownloadRecord
            {
                YouTubeId = youtubeId,
                VideoTitle = customTitle ?? result.VideoTitle,
                DownloadedAt = DateTime.UtcNow.ToString("o")
            };
            await historyRepo.SaveAsync(record, cancellationToken);
        }
        catch (Exception)
        {
            // Silently fail - history save is non-critical
            // Caller still gets successful result
        }

        // 5. Return success with DTO
        return Result<DownloadResultDto>.Success(result);
    }
}
