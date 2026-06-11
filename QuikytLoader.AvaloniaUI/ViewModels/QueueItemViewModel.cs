using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public sealed partial class QueueItemViewModel(QueueItem model, Action<Guid> proceedCallback, bool isInGroup) : QueueEntryViewModel
{
    [NotifyCanExecuteChangedFor(nameof(ProceedCommand))]
    [ObservableProperty] private bool _isSelected = true;
    [ObservableProperty] private bool _isCheckboxEnabled = true;

    private QueueItem Model { get; } = model;

    public Guid QueueItemId => Model.Id;

    public bool IsInGroup => isInGroup;

    public bool CanStartDownload => Model.CanStartDownload;
    public bool CanProceed => IsSelected && IsSelectable && CanStartDownload;
    public bool IsSelectable => Status != DownloadStatus.Disabled;

    public bool IsMetadataLoaded => Model.Metadata is not null;

    public string Url => Model.Source.Url;

    // When Metadata is null show Url instead of Title
    public string? Title => CustomTitle ?? Model.Metadata?.Title ?? Url;
    public string? Channel => Model.Metadata?.Channel;
    public string? Duration => Model.Metadata?.Duration;
    public string? ThumbnailUrl => Model.Metadata?.ThumbnailUrl;

    public DownloadStatus Status => Model.Status;
    public double Progress => Model.Progress;
    public string? ErrorMessage => Model.Error?.Message;

    public string? CustomTitle
    {
        get => Model.CustomTitle;
        set
        {
            if (Model.CustomTitle == value) return;

            Model.CustomTitle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Title));
        }
    }

    public void UpdateFrom(QueueItem item)
    {
        Model.Metadata = item.Metadata;
        OnPropertyChanged(nameof(IsMetadataLoaded));

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Channel));
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(ThumbnailUrl));

        Model.Status = item.Status;
        Model.Progress = item.Progress;
        Model.Error = item.Error;

        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusMessage));
        OnPropertyChanged(nameof(ErrorMessage));

        // Setting `CustomTitle` as `Title` if user hasn't typed anything yet
        if (Status == DownloadStatus.Editing && string.IsNullOrWhiteSpace(CustomTitle))
            CustomTitle = item.Metadata?.Title;
    }

    [RelayCommand(CanExecute = nameof(CanProceed))]
    private void Proceed() => proceedCallback(Model.Id);

    public string StatusMessage => Status switch
    {
        DownloadStatus.Queued => "⏱ Queued",
        DownloadStatus.Pending => "⏸ Pending",
        DownloadStatus.Editing => "⚡ Waiting for title edit",
        DownloadStatus.Downloading => "⏳ Downloading...",
        DownloadStatus.Completed => "✓ Completed",
        DownloadStatus.Failed => "✗ Failed",
        DownloadStatus.Cancelled => "⊘ Cancelled",
        DownloadStatus.Disabled => $"⊘ {Model.Error?.Message ?? "Disabled"}",
        _ => throw new ArgumentOutOfRangeException(nameof(Status), Status, "Unhandled download status")
    };
}
