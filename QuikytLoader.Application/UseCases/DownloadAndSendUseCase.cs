using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
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
    public async Task<DownloadResultDto> ExecuteAsync(
        string url,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Extract YouTube ID
        var youtubeId = await youtubeExtractorService.ExtractVideoIdAsync(url, cancellationToken)
            ?? throw new InvalidOperationException("Failed to extract YouTube video ID from URL");

        // 2. Download video
        var result = customTitle != null
            ? await downloadService.DownloadAsync(url, customTitle, progress, cancellationToken)
            : await downloadService.DownloadAsync(url, progress, cancellationToken);

        // 3. Send to Telegram
        await telegramService.SendAudioAsync(
            result.TempMediaFilePath,
            result.TempThumbnailPath);

        // 4. Save to history
        var record = new DownloadRecord
        {
            YouTubeId = youtubeId,
            VideoTitle = customTitle ?? result.VideoTitle,
            DownloadedAt = DateTime.UtcNow.ToString("o")
        };
        await historyRepo.SaveAsync(record, cancellationToken);

        // 5. Return DTO
        return result;
    }
}
