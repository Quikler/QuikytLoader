using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
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
    /// Gets the existing download record if it exists.
    /// Returns a Result containing the download record if found, null if not found, or an Error on failure.
    /// </summary>
    /// <param name="url">YouTube video URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the download record if found, null if no duplicate exists, or error details if extraction fails</returns>
    public async Task<Result<DownloadEntity?>> GetExistingRecordAsync(string url, CancellationToken cancellationToken = default)
    {
        var youtubeIdResult = await youtubeExtractorService.ExtractVideoIdAsync(url, cancellationToken);
        if (!youtubeIdResult.IsSuccess)
            return Result<DownloadEntity?>.Failure(youtubeIdResult.Error);

        var downloadEntity = await historyRepo.GetByIdAsync(youtubeIdResult.Value, cancellationToken);
        return Result<DownloadEntity?>.Success(downloadEntity);
    }
}
