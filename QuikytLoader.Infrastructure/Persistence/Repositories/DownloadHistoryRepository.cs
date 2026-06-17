using Dapper;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Domain.Entities;

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
            INSERT OR REPLACE INTO DownloadHistory (YoutubeVideoId, VideoTitle, DownloadedAt)
            VALUES (@YoutubeVideoId, @VideoTitle, @DownloadedAt)
            """;

        await connection.ExecuteAsync(upsertSql, new
        {
            downloadEntity.YoutubeVideoId,
            downloadEntity.VideoTitle,
            downloadEntity.DownloadedAt
        });
    }

    public async Task<DownloadHistoryEntity?> GetByYoutubeVideoIdAsync(string youtubeVideoId)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync();

        const string query = """
            SELECT YoutubeVideoId, VideoTitle, DownloadedAt
            FROM DownloadHistory
            WHERE YoutubeVideoId = @YoutubeVideoId
            """;

        var result = await connection.QuerySingleOrDefaultAsync<DownloadHistoryDto>(query, new { YoutubeVideoId = youtubeVideoId });

        if (result is null) return null;

        var createResult = DownloadHistoryEntity.Create(result.YoutubeVideoId, result.VideoTitle, result.DownloadedAt);
        return createResult.IsSuccess ? createResult.Value : null;
    }

    // Should be internal for Dapper.AOT compatibility
    internal record DownloadHistoryDto(string YoutubeVideoId, string VideoTitle, string DownloadedAt);
}
