using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing a download history record
/// </summary>
public class DownloadRecord
{
    /// <summary>
    /// YouTube video ID (11 characters)
    /// </summary>
    public required YouTubeId YouTubeId { get; init; }

    /// <summary>
    /// Video title (custom or original from YouTube)
    /// </summary>
    public required string VideoTitle { get; init; }

    /// <summary>
    /// Timestamp when the video was downloaded and sent to Telegram (ISO 8601 format)
    /// </summary>
    public required string DownloadedAt { get; init; }
}
