using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Check if a YouTube video has already been downloaded
/// </summary>
public class CheckDuplicateUseCase(
    IDownloadHistoryRepository historyRepo,
    IYoutubeExtractorService youtubeExtractorService)
{
    public async Task<Result<DownloadHistoryDto?>> GetExistingRecordAsync(string youtubeUrl, CancellationToken cancellationToken = default)
    {
        var youtubeIdResult = await youtubeExtractorService.GetVideoIdAsync(youtubeUrl, cancellationToken);
        if (!youtubeIdResult.IsSuccess)
            return Result<DownloadHistoryDto?>.Failure(youtubeIdResult.Error);

        var downloadEntity = await historyRepo.GetByIdAsync(youtubeIdResult.Value, cancellationToken);
        if (downloadEntity is null)
            return Result<DownloadHistoryDto?>.Success(null);

        var duplicateResult = new DownloadHistoryDto(downloadEntity.YouTubeId, downloadEntity.VideoTitle, DateTime.Parse(downloadEntity.DownloadedAt));
        return Result<DownloadHistoryDto?>.Success(duplicateResult);
    }
}
