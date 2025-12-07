using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Service for extracting YouTube video IDs from URLs
/// </summary>
public interface IYoutubeExtractorService
{
    /// <summary>
    /// Extracts the YouTube video ID from a given URL
    /// </summary>
    /// <param name="url">The YouTube URL to extract the ID from</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The extracted video ID as a YouTubeId value object, or null if extraction fails</returns>
    Task<YouTubeId?> ExtractVideoIdAsync(string url, CancellationToken cancellationToken = default);
}
