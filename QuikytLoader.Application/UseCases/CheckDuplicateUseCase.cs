using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Check if a YouTube video has already been downloaded
/// </summary>
public class CheckDuplicateUseCase(
    IDownloadHistoryRepository historyRepo,
    IYoutubeExtractorService youtubeExtractorService)
{
    /// <summary>
    /// Gets the existing download record if it exists
    /// </summary>
    /// <param name="url">YouTube video URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The download record if found, otherwise null</returns>
    public async Task<DownloadRecord?> GetExistingRecordAsync(string url, CancellationToken cancellationToken = default)
    {
        var youtubeIdResult = await youtubeExtractorService.ExtractVideoIdAsync(url, cancellationToken);
        if (!youtubeIdResult.IsSuccess) return null;

        return await historyRepo.GetByIdAsync(youtubeIdResult.Value, cancellationToken);
    }
}
