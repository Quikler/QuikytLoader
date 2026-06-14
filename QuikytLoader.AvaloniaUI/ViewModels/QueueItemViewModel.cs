using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public sealed partial class QueueItemViewModel(QueueItem model, Action<Guid> proceedCallback) : QueueEntryViewModel
{
    #region --- NOT EDITABLE BY USER ---

    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyCanExecuteChangedFor(nameof(ProceedCommand))]
    [ObservableProperty] private DownloadStatus _status = model.Status;

    [ObservableProperty] private double _progress = model.Progress;
    [ObservableProperty] private string? _errorMessage = model.Error?.Message;
    [ObservableProperty] private bool _isSelectable = model.CanStartDownload;

    [RelayCommand(CanExecute = nameof(CanProceed))]
    private void Proceed() => proceedCallback(model.Id);

    public string StatusMessage => Status switch
    {
        DownloadStatus.Queued => "⏱ Queued",
        DownloadStatus.Pending => "⏸ Pending",
        DownloadStatus.Editing => "⚡ Waiting for title edit",
        DownloadStatus.Downloading => "⏳ Downloading...",
        DownloadStatus.Completed => "✓ Completed",
        DownloadStatus.Failed => "✗ Failed",
        DownloadStatus.Cancelled => "⊘ Cancelled",
        DownloadStatus.Disabled => $"⊘ {model.Error?.Message ?? "Disabled"}",
        _ => throw new ArgumentOutOfRangeException(nameof(Status), Status, "Unhandled download status")
    };

    public Guid QueueItemId => model.Id;
    public bool CanProceed => IsSelected && IsSelectable;

    #region --- Metadata (Updates UI when `UpdateFrom` is executed) ---

    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Channel))]
    [NotifyPropertyChangedFor(nameof(Duration))]
    [NotifyPropertyChangedFor(nameof(ThumbnailUrl))]
    [NotifyPropertyChangedFor(nameof(IsMetadataLoaded))]
    [ObservableProperty] private VideoMetadata? _metadata;

    // When Metadata is null show Url instead of Title
    public string? Title => CustomTitle ?? model.Metadata?.Title ?? Url;
    public string? Channel => model.Metadata?.Channel;
    public string? Duration => model.Metadata?.Duration;
    public string? ThumbnailUrl => model.Metadata?.ThumbnailUrl;
    public bool IsMetadataLoaded => model.Metadata is not null;

    // Only initialized once, because `model.Source` is `init`
    public string Url => model.Source.Url;

    #endregion

    #endregion

    #region --- EDIABLE BY USER ---

    public string? CustomTitle
    {
        get => model.CustomTitle;
        set
        {
            if (model.CustomTitle == value) return;

            model.CustomTitle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Title));
        }
    }

    [NotifyCanExecuteChangedFor(nameof(ProceedCommand))]
    [ObservableProperty] private bool _isSelected;

    #endregion

    public void UpdateFrom(QueueItem item)
    {
        Metadata = item.Metadata;

        Status = item.Status;
        Progress = item.Progress;
        ErrorMessage = item.Error?.Message;
        IsSelectable = item.CanStartDownload;

        // Setting `CustomTitle` as `Title` if user hasn't typed anything yet
        if (Status == DownloadStatus.Editing && string.IsNullOrWhiteSpace(CustomTitle))
            CustomTitle = item.Metadata?.Title;
    }
}
