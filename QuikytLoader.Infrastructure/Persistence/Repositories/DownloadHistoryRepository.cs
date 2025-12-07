using System.Diagnostics;
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
    public async Task SaveAsync(DownloadRecord record, CancellationToken cancellationToken = default)
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
                YouTubeId = record.YouTubeId.Value,
                record.VideoTitle,
                record.DownloadedAt
            }, cancellationToken: cancellationToken)
        );
    }

    public async Task<DownloadRecord?> GetByIdAsync(YouTubeId id, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.GetConnectionAsync(cancellationToken);

        const string query = """
            SELECT YouTubeId, VideoTitle, DownloadedAt
            FROM DownloadHistory
            WHERE YouTubeId = @YouTubeId
            """;

        var result = await connection.QuerySingleOrDefaultAsync<DownloadRecordDto>(
            new CommandDefinition(query, new { YouTubeId = id.Value }, cancellationToken: cancellationToken)
        );

        if (result is null) return null;

        return new DownloadRecord
        {
            YouTubeId = new YouTubeId(result.YouTubeId),
            VideoTitle = result.VideoTitle,
            DownloadedAt = result.DownloadedAt
        };
    }

    public async Task<IEnumerable<DownloadRecord>> GetAllAsync(CancellationToken cancellationToken = default)
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

        return results.Select(r => new DownloadRecord
        {
            YouTubeId = new YouTubeId(r.YouTubeId),
            VideoTitle = r.VideoTitle,
            DownloadedAt = r.DownloadedAt
        });
    }

    public async Task<string> GetThumbnailUrlAsync(YouTubeId youtubeId, CancellationToken cancellationToken = default)
    {
        // Try to get thumbnail URL from yt-dlp
        var thumbnailUrl = await TryGetThumbnailFromYtDlpAsync(youtubeId.Value, cancellationToken);
        if (!string.IsNullOrEmpty(thumbnailUrl))
            return thumbnailUrl;

        // Fallback to default YouTube thumbnail URLs
        // Try maxresdefault first (highest quality), then fallback to hqdefault
        return $"https://img.youtube.com/vi/{youtubeId.Value}/maxresdefault.jpg";
    }

    private static async Task<string?> TryGetThumbnailFromYtDlpAsync(string youtubeId, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--print thumbnail --skip-download \"https://youtube.com/watch?v={youtubeId}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return null;

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                return null;

            var output = await outputTask;
            var url = output.Trim();

            // Validate it's a proper URL
            return Uri.TryCreate(url, UriKind.Absolute, out _) ? url : null;
        }
        catch
        {
            return null;
        }
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
