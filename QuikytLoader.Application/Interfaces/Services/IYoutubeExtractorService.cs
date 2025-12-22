using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Service for extracting YouTube video IDs from URLs
/// </summary>
public interface IYoutubeExtractorService
{
    /// <summary>
    /// Gets video ID from a given URL.
    /// </summary>
    Task<Result<YouTubeId>> GetVideoIdAsync(string youtubeUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets video title without downloading.
    /// </summary>
    Task<Result<string>> GetVideoTitleAsync(string youtubeUrl, CancellationToken cancellationToken = default);
}
