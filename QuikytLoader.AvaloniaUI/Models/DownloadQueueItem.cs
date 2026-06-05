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
    /// Current status of this download
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    private DownloadStatus _status = DownloadStatus.Queued;

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
        DownloadStatus.Queued => "⏱ Queued",
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
    [NotifyPropertyChangedFor(nameof(CurrentTitle))]
    private string? _customTitle;

    public string? CurrentTitle
    {
        get
        {
            if (!VideoMetadata.IsLoaded)
                return VideoMetadata.Url;

            return string.IsNullOrWhiteSpace(CustomTitle)
                ? VideoMetadata.Title
                : CustomTitle;
        }
    }

    /// <summary>
    /// Group id this item belongs to. Set by DownloadQueueManager on enqueue.
    /// </summary>
    [ObservableProperty]
    private string? _groupId;

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
        IsProceedButtonEnabled = false;
    }

    public VideoMetadataViewModel VideoMetadata { get; } = new();

    public void ApplyMetadata(Result<VideoMetadataDto> result)
    {
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error.Message;
            return;
        }

        var metadata = result.Value;
        VideoMetadata.Url = metadata.Url;
        VideoMetadata.VideoId = metadata.VideoId;
        VideoMetadata.Title = metadata.Title;
        VideoMetadata.Channel = metadata.Channel;
        VideoMetadata.Duration = metadata.Duration;
        VideoMetadata.ThumbnailUrl = metadata.ThumbnailUrl;
        VideoMetadata.IsAvailable = metadata.IsAvailable;
        VideoMetadata.UnavailableReason = metadata.UnavailableReason;
        VideoMetadata.IsLoaded = true;

        // Only populate custom title if user hasn't started editing yet
        if (Status == DownloadStatus.Editing && string.IsNullOrWhiteSpace(CustomTitle))
            CustomTitle = metadata.Title;

        OnPropertyChanged(nameof(CurrentTitle));
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"[{StatusMessage}] ");

        if (!string.IsNullOrWhiteSpace(CurrentTitle))
            sb.Append($"{CurrentTitle}");

        if (Status == DownloadStatus.Downloading)
            sb.Append($" ({Progress:F0}%)");

        if (VideoMetadata.IsLoaded)
            sb.Append(VideoMetadata.ToString());

        if (Status == DownloadStatus.Failed && !string.IsNullOrWhiteSpace(ErrorMessage))
            sb.Append($" - Error: {ErrorMessage}");

        return sb.ToString();
    }
}
