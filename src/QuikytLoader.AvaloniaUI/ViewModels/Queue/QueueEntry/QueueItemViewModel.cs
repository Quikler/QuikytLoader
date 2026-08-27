using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry.Subtitles;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

public partial class QueueItemViewModel : QueueEntryViewModel
{
    protected QueueItem Model { get; }

    private readonly Action<Guid> _proceedCallback;
    private readonly Action<Guid> _cancelCallback;

    public QueueItemViewModel(
        QueueItem model,
        IUserSettings userSettings,
        Action<Guid> proceedCallback,
        Action<Guid> cancelCallback,
        FetchManualSubtitlesUseCase fetchManualSubtitlesUseCase,
        FetchAutoSubtitlesUseCase fetchAutoSubtitlesUseCase,
        CancelSubtitlesUseCase cancelSubtitlesUseCase)
    {
        Model = model;
        QueueItemSubtitlesViewModel = new(
            model.Subtitles,
            userSettings,
            fetchManualSubtitlesUseCase,
            fetchAutoSubtitlesUseCase,
            cancelSubtitlesUseCase);

        _proceedCallback = proceedCallback;
        _cancelCallback = cancelCallback;

        RefreshInternal();
    }

    public QueueItemSubtitlesViewModel QueueItemSubtitlesViewModel { get; }

    #region --- NOT EDITABLE BY USER ---

    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [ObservableProperty] private DownloadStatus _status;

    [ObservableProperty] private double _progress;
    [ObservableProperty] private string? _errorMessage;

    [RelayCommand]
    private void Proceed() => _proceedCallback(Model.Id);

    [RelayCommand]
    private void Cancel() => _cancelCallback(Model.Id);

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

    public Guid QueueItemId => Model.Id;
    public virtual bool CanProceed => Model.CanStartDownload;
    public virtual bool CanCancel => Model.CanCancel;
    public bool CanEdit => Model.CanEdit;

    #region --- Metadata (Updates UI when `Refresh` is executed) ---

    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Channel))]
    [NotifyPropertyChangedFor(nameof(Duration))]
    [NotifyPropertyChangedFor(nameof(CoverThumbnailUrl))]
    [ObservableProperty] private VideoMetadata? _metadata;

    // When Metadata is null show Url instead of Title
    public string? Title => CustomTitle ?? Model.Metadata?.Title ?? Url;
    public string? Channel => Model.Metadata?.Channel;
    public string? Duration
    {
        get
        {
            if (Model.Metadata?.DurationInSeconds is null)
                return null;

            var ts = Model.Metadata.DurationInSeconds;
            return ts.TotalHours >= 1
                ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes}:{ts.Seconds:D2}";
        }
    }
    public string? CoverThumbnailUrl => $"https://i.ytimg.com/vi/{Model.Source.YoutubeVideoId}/default.jpg";

    // Only initialized once, because `model.Source` is `init`
    public string Url => Model.Source.YoutubeVideoUrl;

    #endregion

    #endregion

    #region --- EDIABLE BY USER ---

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

    #endregion

    public virtual void Refresh() => RefreshInternal();

    // This exists to avoid "CA2214: Do not call overridable methods in constructors"
    private void RefreshInternal()
    {
        Metadata = Model.Metadata;

        Status = Model.Status;
        Progress = Model.Progress;
        ErrorMessage = Model.Error?.Message;

        // Setting `CustomTitle` as `Title` if user hasn't typed anything yet
        if (CanEdit && string.IsNullOrWhiteSpace(CustomTitle))
            CustomTitle = Model.Metadata?.Title;
    }
}
