using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing a download history record
/// </summary>
/// <param name="YouTubeId">YouTube video ID (11 characters)</param>
/// <param name="VideoTitle">Video title (custom or original from YouTube)</param>
/// <param name="DownloadedAt">Timestamp when the video was downloaded and sent to Telegram (ISO 8601 format)</param>
public record DownloadHistoryEntity(
    YouTubeId YouTubeId,
    string VideoTitle,
    string DownloadedAt)
{
    public static DownloadHistoryEntity Create(string youtubeId, string videoTitle, string downloadedAt)
    {
        var youtubeIdResult = YouTubeId.Create(youtubeId);

        if (!youtubeIdResult.IsSuccess)
            throw new InvalidOperationException(
                $"Database integrity violation: {youtubeIdResult.Error.Message}. " +
                "This indicates database constraint enforcement failed.");

        return new DownloadHistoryEntity(youtubeIdResult.Value, videoTitle, downloadedAt);
    }
}
