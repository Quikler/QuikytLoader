using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.Interfaces.Repositories;

/// <summary>
/// Repository pattern for download history persistence
/// </summary>
public interface IDownloadHistoryRepository
{
    /// <summary>
    /// Saves a download history record. Updates existing record if YouTubeId already exists.
    /// </summary>
    /// <param name="downloadEntity">The record to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveAsync(DownloadEntity downloadEntity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a download record by YouTube ID
    /// </summary>
    /// <param name="id">The YouTube video ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The record if found, otherwise null</returns>
    Task<DownloadEntity?> GetByIdAsync(YouTubeId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all download records
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all download records</returns>
    Task<IEnumerable<DownloadEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the thumbnail URL for a YouTube video
    /// Tries to get from yt-dlp, falls back to default YouTube thumbnail URLs
    /// </summary>
    /// <param name="youtubeId">The YouTube video ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Thumbnail URL</returns>
    Task<string> GetThumbnailUrlAsync(YouTubeId youtubeId, CancellationToken cancellationToken = default);
}
