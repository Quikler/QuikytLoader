using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using QuikytLoader.Models;

namespace QuikytLoader.Services;

/// <summary>
/// Service for managing download history using SQLite database with Dapper ORM.
/// </summary>
public class DownloadHistoryService(IDbConnectionService dbConnectionService) : IDownloadHistoryService
{

    public async Task<DownloadHistoryRecord?> CheckDuplicateAsync(string youtubeId, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionService.GetConnectionAsync(cancellationToken);

        const string query = """
            SELECT YouTubeId, VideoTitle, DownloadedAt, TelegramMessageId
            FROM DownloadHistory
            WHERE YouTubeId = @YouTubeId
            """;

        return await connection.QuerySingleOrDefaultAsync<DownloadHistoryRecord>(
            new CommandDefinition(query, new { YouTubeId = youtubeId }, cancellationToken: cancellationToken)
        );
    }

    public async Task SaveHistoryAsync(DownloadHistoryRecord record, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionService.GetConnectionAsync(cancellationToken);

        // Use INSERT OR REPLACE to handle both insert and update cases
        const string upsertSql = """
            INSERT OR REPLACE INTO DownloadHistory (YouTubeId, VideoTitle, DownloadedAt, TelegramMessageId)
            VALUES (@YouTubeId, @VideoTitle, @DownloadedAt, @TelegramMessageId)
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(upsertSql, record, cancellationToken: cancellationToken)
        );
    }

    public async Task<string> GetThumbnailUrlAsync(string youtubeId, CancellationToken cancellationToken = default)
    {
        // Try to get thumbnail URL from yt-dlp
        var thumbnailUrl = await TryGetThumbnailFromYtDlpAsync(youtubeId, cancellationToken);
        if (!string.IsNullOrEmpty(thumbnailUrl))
            return thumbnailUrl;

        // Fallback to default YouTube thumbnail URLs
        // Try maxresdefault first (highest quality), then fallback to hqdefault
        return $"https://img.youtube.com/vi/{youtubeId}/maxresdefault.jpg";
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
}
