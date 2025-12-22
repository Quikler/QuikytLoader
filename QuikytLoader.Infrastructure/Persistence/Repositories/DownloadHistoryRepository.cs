using Dapper;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing download history using SQLite database with Dapper ORM
/// </summary>
internal class DownloadHistoryRepository(IDbConnectionFactory dbConnectionFactory) : IDownloadHistoryRepository
{
    public async Task UpsertAsync(DownloadHistoryEntity downloadEntity, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync(cancellationToken);

        // Use INSERT OR REPLACE to handle both insert and update cases
        const string upsertSql = """
            INSERT OR REPLACE INTO DownloadHistory (YouTubeId, VideoTitle, DownloadedAt)
            VALUES (@YouTubeId, @VideoTitle, @DownloadedAt)
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(upsertSql, new
            {
                YouTubeId = downloadEntity.YouTubeId.Id,
                downloadEntity.VideoTitle,
                downloadEntity.DownloadedAt
            }, cancellationToken: cancellationToken)
        );
    }

    public async Task<DownloadHistoryEntity?> GetByIdAsync(YouTubeId id, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync(cancellationToken);

        const string query = """
            SELECT YouTubeId, VideoTitle, DownloadedAt
            FROM DownloadHistory
            WHERE YouTubeId = @YouTubeId
            """;

        var result = await connection.QuerySingleOrDefaultAsync<DownloadRecordDto>(
            new CommandDefinition(query, new { YouTubeId = id.Id }, cancellationToken: cancellationToken)
        );

        if (result is null) return null;

        return DownloadHistoryEntity.Create(result.YouTubeId, result.VideoTitle, result.DownloadedAt);
    }

    public async Task<IEnumerable<DownloadHistoryEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync(cancellationToken);

        const string query = """
            SELECT YouTubeId, VideoTitle, DownloadedAt
            FROM DownloadHistory
            ORDER BY DownloadedAt DESC
            """;

        var results = await connection.QueryAsync<DownloadRecordDto>(
            new CommandDefinition(query, cancellationToken: cancellationToken)
        );

        return results.Select(r =>
            DownloadHistoryEntity.Create(r.YouTubeId, r.VideoTitle, r.DownloadedAt));
    }

    /// <summary>
    /// Internal DTO for Dapper mapping from database
    /// </summary>
    private class DownloadRecordDto
    {
        public required string YouTubeId { get; init; }
        public required string VideoTitle { get; init; }
        public required string DownloadedAt { get; init; }
    }
}
