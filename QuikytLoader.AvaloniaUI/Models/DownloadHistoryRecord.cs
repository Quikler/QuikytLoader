namespace QuikytLoader.Models;

/// <summary>
/// Represents a record of a successfully downloaded and sent YouTube video.
/// </summary>
public class DownloadHistoryRecord
{
    /// <summary>
    /// YouTube video ID (11 characters).
    /// </summary>
    public required string YouTubeId { get; init; }

    /// <summary>
    /// Video title (custom or original from YouTube).
    /// </summary>
    public required string VideoTitle { get; init; }

    /// <summary>
    /// Timestamp when the video was downloaded and sent to Telegram (ISO 8601 format).
    /// </summary>
    public required string DownloadedAt { get; init; }
}
