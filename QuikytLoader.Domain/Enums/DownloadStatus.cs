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
    /// Item is waiting for user to edit the title before proceeding
    /// </summary>
    Editing,

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
    Cancelled,

    /// <summary>
    /// Item is permanently disabled (e.g. already downloaded, unavailable video, duplicate in another playlist).
    /// Disabled items are never processed by the queue and cannot be selected.
    /// </summary>
    Disabled
}
