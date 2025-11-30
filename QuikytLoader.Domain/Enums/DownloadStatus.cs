namespace QuikytLoader.Domain.Enums;

/// <summary>
/// Represents the current status of a download queue item
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// Download is queued and waiting to be processed
    /// </summary>
    Pending,

    /// <summary>
    /// Download is currently in progress
    /// </summary>
    Downloading,

    /// <summary>
    /// Download completed successfully and was sent to Telegram
    /// </summary>
    Completed,

    /// <summary>
    /// Download failed due to an error
    /// </summary>
    Failed,

    /// <summary>
    /// Download was cancelled by the user
    /// </summary>
    Cancelled
}
