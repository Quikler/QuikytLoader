namespace QuikytLoader.Application.DTOs;

/// <summary>
/// Data transfer object for download history record (for UI binding)
/// </summary>
public class DownloadHistoryRecordDto
{
    /// <summary>
    /// YouTube video ID (11 characters)
    /// </summary>
    public required string YouTubeId { get; init; }

    /// <summary>
    /// Video title (custom or original from YouTube)
    /// </summary>
    public required string VideoTitle { get; init; }

    /// <summary>
    /// Timestamp when the video was downloaded and sent to Telegram (ISO 8601 format)
    /// </summary>
    public required string DownloadedAt { get; init; }
}
