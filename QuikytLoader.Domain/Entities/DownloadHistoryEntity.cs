using QuikytLoader.Domain.Common;

namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing a download history record
/// </summary>
/// <param name="YoutubeVideoId">Youtube video ID</param>
/// <param name="VideoTitle">Video title (custom or original from Youtube)</param>
/// <param name="DownloadedAt">Timestamp when the video was downloaded and sent to Telegram (ISO 8601 format)</param>
public record DownloadHistoryEntity(
    string YoutubeVideoId,
    string VideoTitle,
    string DownloadedAt)
{
    public static Result<DownloadHistoryEntity> Create(string youtubeVideoId, string videoTitle, string downloadedAt)
        => new DownloadHistoryEntity(youtubeVideoId, videoTitle, downloadedAt);
}
