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
    Result<YouTubeId> GetVideoId(string youtubeUrl);

    /// <summary>
    /// Gets playlist ID from a given URL.
    /// </summary>
    Result<string> GetPlaylistId(string youtubePlaylistUrl);

    /// <summary>
    /// Gets video title without downloading.
    /// </summary>
    Task<Result<string>> GetVideoTitleAsync(string youtubeUrl, CancellationToken cancellationToken = default);
}
