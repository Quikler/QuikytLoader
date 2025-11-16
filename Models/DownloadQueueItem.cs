using CommunityToolkit.Mvvm.ComponentModel;

namespace QuikytLoader.Models;

/// <summary>
/// Represents a single item in the download queue
/// Observable to support UI binding and real-time updates
/// </summary>
public partial class DownloadQueueItem : ObservableObject
{
    /// <summary>
    /// The YouTube URL to download
    /// </summary>
    [ObservableProperty]
    private string _url = string.Empty;

    /// <summary>
    /// Current status of this download
    /// </summary>
    [ObservableProperty]
    private DownloadStatus _status = DownloadStatus.Pending;

    /// <summary>
    /// Download progress (0-100)
    /// </summary>
    [ObservableProperty]
    private double _progress = 0;

    /// <summary>
    /// Error message if status is Failed
    /// </summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Status message for display (e.g., "Downloading...", "Sending to Telegram...")
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Pending";

    /// <summary>
    /// Result of the download operation (populated when status is Completed)
    /// </summary>
    public DownloadResult? DownloadResult { get; set; }
}
