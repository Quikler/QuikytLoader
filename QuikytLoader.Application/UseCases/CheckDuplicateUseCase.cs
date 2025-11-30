using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Check if a YouTube video has already been downloaded
/// </summary>
public class CheckDuplicateUseCase
{
    private readonly IDownloadHistoryRepository _historyRepo;
    private readonly IYoutubeExtractor _extractor;

    public CheckDuplicateUseCase(
        IDownloadHistoryRepository historyRepo,
        IYoutubeExtractor extractor)
    {
        _historyRepo = historyRepo;
        _extractor = extractor;
    }

    /// <summary>
    /// Checks if a video has already been downloaded
    /// </summary>
    /// <param name="url">YouTube video URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if video exists in history, otherwise false</returns>
    public async Task<bool> ExistsAsync(string url, CancellationToken cancellationToken = default)
    {
        var youtubeId = await _extractor.ExtractVideoIdAsync(url, cancellationToken);
        if (youtubeId == null)
        {
            return false;
        }

        return await _historyRepo.ExistsAsync(youtubeId, cancellationToken);
    }

    /// <summary>
    /// Gets the existing download record if it exists
    /// </summary>
    /// <param name="url">YouTube video URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The download record if found, otherwise null</returns>
    public async Task<DownloadRecord?> GetExistingRecordAsync(string url, CancellationToken cancellationToken = default)
    {
        var youtubeId = await _extractor.ExtractVideoIdAsync(url, cancellationToken);
        if (youtubeId == null)
        {
            return null;
        }

        return await _historyRepo.GetByIdAsync(youtubeId, cancellationToken);
    }
}
