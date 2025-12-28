using Dapper;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;
using QuikytLoader.Infrastructure.Persistence.DTOs;

namespace QuikytLoader.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing download history using SQLite database with Dapper ORM.
/// Uses simple Dapper overloads (not CommandDefinition) for Dapper.AOT compatibility.
/// </summary>
internal class DownloadHistoryRepository(IDbConnectionFactory dbConnectionFactory) : IDownloadHistoryRepository
{
    public async Task UpsertAsync(DownloadHistoryEntity downloadEntity, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync(cancellationToken);
        const string upsertSql = """
            INSERT OR REPLACE INTO DownloadHistory (YouTubeId, VideoTitle, DownloadedAt)
            VALUES (@YouTubeId, @VideoTitle, @DownloadedAt)
            """;

        await connection.ExecuteAsync(upsertSql, new
        {
            YouTubeId = downloadEntity.YouTubeId.Id,
            downloadEntity.VideoTitle,
            downloadEntity.DownloadedAt
        });
    }

    public async Task<DownloadHistoryEntity?> GetByIdAsync(YouTubeId id, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync(cancellationToken);
        const string query = """
            SELECT YouTubeId, VideoTitle, DownloadedAt
            FROM DownloadHistory
            WHERE YouTubeId = @YouTubeId
            """;

        var result = await connection.QuerySingleOrDefaultAsync<DownloadRecordDto>(query, new { YouTubeId = id.Id });
        if (result is null) return null;

        return DownloadHistoryEntity.Create(result.YouTubeId, result.VideoTitle, result.DownloadedAt);
    }
}
