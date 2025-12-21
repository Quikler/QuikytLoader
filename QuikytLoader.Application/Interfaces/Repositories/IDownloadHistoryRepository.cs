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
    Task UpsertAsync(DownloadEntity downloadEntity, CancellationToken cancellationToken = default);

    Task<DownloadEntity?> GetByIdAsync(YouTubeId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<DownloadEntity>> GetAllAsync(CancellationToken cancellationToken = default);
}
