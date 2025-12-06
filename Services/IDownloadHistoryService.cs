using System.Threading;
using System.Threading.Tasks;
using QuikytLoader.Models;

namespace QuikytLoader.Services;

/// <summary>
/// Service for managing download history stored in SQLite database.
/// </summary>
public interface IDownloadHistoryService
{
    /// <summary>
    /// Checks if a YouTube video has already been downloaded.
    /// </summary>
    /// <param name="youtubeId">The YouTube video ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The existing record if found, otherwise null.</returns>
    Task<DownloadHistoryRecord?> CheckDuplicateAsync(string youtubeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a download history record. Updates existing record if YouTubeId already exists.
    /// </summary>
    /// <param name="record">The record to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveHistoryAsync(DownloadHistoryRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the thumbnail URL for a YouTube video.
    /// Tries to get from yt-dlp, falls back to default YouTube thumbnail URLs.
    /// </summary>
    /// <param name="youtubeId">The YouTube video ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Thumbnail URL.</returns>
    Task<string> GetThumbnailUrlAsync(string youtubeId, CancellationToken cancellationToken = default);
}
