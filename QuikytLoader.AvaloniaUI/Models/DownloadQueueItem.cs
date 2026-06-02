using System;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.Models;

/// <summary>
/// Represents a single item in the download queue
/// Observable to support UI binding and real-time updates
/// UI-specific model not part of Domain/Application layers
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
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
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
    /// Status message derived from current Status (or DisabledReason when disabled).
    /// </summary>
    public string StatusMessage => Status switch
    {
        DownloadStatus.Pending => "⏸ Pending",
        DownloadStatus.Editing => "⚡ Waiting for title edit",
        DownloadStatus.Downloading => "⏳ Downloading...",
        DownloadStatus.Completed => "✓ Completed",
        DownloadStatus.Failed => "✗ Failed",
        DownloadStatus.Cancelled => "⊘ Cancelled",
        DownloadStatus.Disabled => $"⊘ {DisabledReason ?? "Disabled"}",
        _ => throw new ArgumentOutOfRangeException(nameof(Status), Status, "Unhandled download status")
    };

    /// <summary>
    /// Optional custom title for the output file (if null, uses video title)
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private string? _customTitle;

    /// <summary>
    /// Fetched video title (separate from CustomTitle which is user-edited)
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private string? _videoTitle;

    /// <summary>
    /// YouTube channel name
    /// </summary>
    [ObservableProperty]
    private string? _channelName;

    /// <summary>
    /// Formatted duration string (e.g., "3:45")
    /// </summary>
    [ObservableProperty]
    private string? _duration;

    /// <summary>
    /// YouTube thumbnail URL for async image loading
    /// </summary>
    [ObservableProperty]
    private string? _thumbnailUrl;

    /// <summary>
    /// Tracks whether metadata fetch completed (for UI loading state)
    /// </summary>
    [ObservableProperty]
    private bool _isMetadataLoaded;

    /// <summary>
    /// Tracks whether metadata fetch failed (for error icon)
    /// </summary>
    [ObservableProperty]
    private bool _hasMetadataError;

    /// <summary>
    /// Display title: shows CustomTitle if set, otherwise VideoTitle
    /// </summary>
    public string? DisplayTitle =>
        string.IsNullOrWhiteSpace(CustomTitle) ? VideoTitle : CustomTitle;

    /// <summary>
    /// Group id this item belongs to. Set by DownloadQueueManager on enqueue.
    /// </summary>
    [ObservableProperty]
    private string? _groupId;

    /// <summary>
    /// 11-char YouTube video id (populated from playlist entry or metadata fetch). Used for cross-playlist dedup.
    /// </summary>
    [ObservableProperty]
    private string? _youtubeId;

    /// <summary>
    /// Reason the item is hard-disabled (shown in StatusMessage when Status == Disabled).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private string? _disabledReason;

    [ObservableProperty]
    private bool _isCheckboxEnabled = true;

    /// <summary>
    /// Whether this item is selected in its playlist group (ignored for non-grouped items).
    /// </summary>
    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private bool _isProceedButtonEnabled = true;

    /// <summary>
    /// Whether this item is part of a YouTube playlist (drives selection checkbox visibility).
    /// </summary>
    [ObservableProperty]
    private bool _isInPlaylist;

    public void SetAsPending()
    {
        DisabledReason = "Already proceeded";
        Status = DownloadStatus.Pending;
        IsCheckboxEnabled = false;
        IsProceedButtonEnabled = false;
    }

    public void SetAsDisabled(string reason)
    {
        DisabledReason = reason;
        Status = DownloadStatus.Disabled;
        IsSelected = false;
    }

    public void ApplyMetadata(Result<VideoMetadataDto> result)
    {
        if (!result.IsSuccess)
        {
            HasMetadataError = true;
            return;
        }

        var metadata = result.Value;
        VideoTitle = metadata.Title;
        ChannelName = metadata.Channel;
        Duration = metadata.Duration;
        ThumbnailUrl = metadata.ThumbnailUrl;
        IsMetadataLoaded = true;

        // Only populate custom title if user hasn't started editing yet
        if (Status == DownloadStatus.Editing && string.IsNullOrWhiteSpace(CustomTitle))
            CustomTitle = metadata.Title;
    }


    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"[{StatusMessage}] ");

        if (!string.IsNullOrWhiteSpace(DisplayTitle))
            sb.Append($"{DisplayTitle}");
        else
            sb.Append(Url);

        if (Status == DownloadStatus.Downloading)
            sb.Append($" ({Progress:F0}%)");

        if (!string.IsNullOrWhiteSpace(ChannelName))
            sb.Append($" - {ChannelName}");

        if (!string.IsNullOrWhiteSpace(Duration))
            sb.Append($" [{Duration}]");

        if (Status == DownloadStatus.Failed && !string.IsNullOrWhiteSpace(ErrorMessage))
            sb.Append($" - Error: {ErrorMessage}");

        return sb.ToString();
    }
}
