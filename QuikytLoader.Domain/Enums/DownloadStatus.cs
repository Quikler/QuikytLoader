namespace QuikytLoader.Domain.Enums;

/// <summary>
/// Represents the current status of a download queue item
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// Item is visible in a playlist group in the UI, waiting for the user to trigger processing.
    /// </summary>
    Queued,

    /// <summary>
    /// Item has been submitted for processing and is waiting to be processed by a processing function
    /// (e.g processQueueItem)
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
