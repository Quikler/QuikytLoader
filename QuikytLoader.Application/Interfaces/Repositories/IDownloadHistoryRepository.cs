using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.Interfaces.Repositories;

/// <summary>
/// Repository for download history persistence
/// </summary>
public interface IDownloadHistoryRepository
{
    /// <summary>
    /// Upserts a download history record. Inserts new record or updates existing record if YouTubeId already exists.
    /// </summary>
    Task UpsertAsync(DownloadHistoryEntity downloadEntity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a download record by YouTube ID.
    /// </summary>
    Task<DownloadHistoryEntity?> GetByIdAsync(YouTubeId id, CancellationToken cancellationToken = default);
}
