using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Repositories;

/// <summary>
/// Repository for download history persistence
/// </summary>
/// <remarks>
/// CancellationToken not supported - Dapper.AOT doesn't intercept CommandDefinition yet.
/// Tracking: https://github.com/DapperLib/DapperAOT/pull/153
/// </remarks>
public interface IDownloadHistoryRepository
{
    /// <summary>
    /// Upserts a download history record. Inserts new record or updates existing record if YoutubeVideoId already exists.
    /// </summary>
    Task UpsertAsync(DownloadHistoryEntity downloadEntity);

    /// <summary>
    /// Gets a download record by Youtube ID.
    /// </summary>
    Task<DownloadHistoryEntity?> GetByYoutubeVideoIdAsync(string youtubeVideoId);
}
