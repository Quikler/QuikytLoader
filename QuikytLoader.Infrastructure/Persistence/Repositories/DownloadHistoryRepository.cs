using Dapper;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing download history using SQLite database with Dapper ORM.
/// </summary>
/// <remarks>
/// CancellationToken not supported in Dapper queries - Dapper.AOT doesn't intercept CommandDefinition yet.
/// Tracking: https://github.com/DapperLib/DapperAOT/pull/153
/// </remarks>
internal class DownloadHistoryRepository(IDbConnectionFactory dbConnectionFactory) : IDownloadHistoryRepository
{
    public async Task UpsertAsync(DownloadHistoryEntity downloadEntity)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync();
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

    public async Task<DownloadHistoryEntity?> GetByIdAsync(YouTubeId id)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync();
        const string query = """
            SELECT YouTubeId, VideoTitle, DownloadedAt
            FROM DownloadHistory
            WHERE YouTubeId = @YouTubeId
            """;

        var result = await connection.QuerySingleOrDefaultAsync<DownloadHistoryDto>(query, new { YouTubeId = id.Id });
        if (result is null) return null;

        return DownloadHistoryEntity.Create(result.YouTubeId, result.VideoTitle, result.DownloadedAt);
    }

    // Should be internal for Dapper.AOT compatibility
    internal record DownloadHistoryDto(string YouTubeId, string VideoTitle, string DownloadedAt);
}
