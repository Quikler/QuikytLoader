using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Service for extracting YouTube video IDs from URLs
/// </summary>
public interface IYoutubeExtractorService
{
    /// <summary>
    /// Extracts the YouTube video ID from a given URL.
    /// Returns a Result containing the YouTubeId on success, or an Error on failure.
    /// </summary>
    /// <param name="url">The YouTube URL to extract the ID from</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Result containing the extracted video ID, or error details if extraction fails</returns>
    Task<Result<YouTubeId>> ExtractVideoIdAsync(string url, CancellationToken cancellationToken = default);
}
