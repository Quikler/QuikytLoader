using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Download YouTube video, save to history, send to Telegram
/// Orchestrates multiple services to complete the workflow
/// </summary>
public class DownloadAndSendUseCase
{
    private readonly IYouTubeDownloadService _downloadService;
    private readonly IDownloadHistoryRepository _historyRepo;
    private readonly ITelegramBotService _telegramService;
    private readonly IYoutubeExtractor _extractor;

    public DownloadAndSendUseCase(
        IYouTubeDownloadService downloadService,
        IDownloadHistoryRepository historyRepo,
        ITelegramBotService telegramService,
        IYoutubeExtractor extractor)
    {
        _downloadService = downloadService;
        _historyRepo = historyRepo;
        _telegramService = telegramService;
        _extractor = extractor;
    }

    public async Task<DownloadResultDto> ExecuteAsync(
        string url,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Extract YouTube ID
        var youtubeId = await _extractor.ExtractVideoIdAsync(url, cancellationToken)
            ?? throw new InvalidOperationException("Failed to extract YouTube video ID from URL");

        // 2. Download video
        var result = customTitle != null
            ? await _downloadService.DownloadAsync(url, customTitle, progress, cancellationToken)
            : await _downloadService.DownloadAsync(url, progress, cancellationToken);

        // 3. Send to Telegram
        await _telegramService.SendAudioAsync(
            result.TempMediaFilePath,
            result.TempThumbnailPath);

        // 4. Save to history
        var record = new DownloadRecord
        {
            YouTubeId = youtubeId,
            VideoTitle = customTitle ?? Path.GetFileNameWithoutExtension(result.TempMediaFilePath),
            DownloadedAt = DateTime.UtcNow.ToString("o")
        };
        await _historyRepo.SaveAsync(record, cancellationToken);

        // 5. Return DTO
        return result;
    }
}
