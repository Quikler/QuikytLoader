using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class QueueItemViewModel : QueueEntryViewModel
{
    protected QueueItem Model { get; }
    private readonly Action<Guid> _proceedCallback;
    private readonly Action<Guid> _cancelCallback;
    private readonly Action<Guid> _fetchSubtitlesCallback;
    private readonly Action<Guid> _cancelSubtitlesCallback;

#pragma warning disable IDE0290 // Use primary constructor
    public QueueItemViewModel(QueueItem model,
        Action<Guid> proceedCallback,
        Action<Guid> cancelCallback,
        Action<Guid> fetchSubtitlesCallback,
        Action<Guid> cancelSubtitlesCallback)
#pragma warning restore IDE0290 // Use primary constructor
    {
        Model = model;
        _proceedCallback = proceedCallback;
        _cancelCallback = cancelCallback;
        _fetchSubtitlesCallback = fetchSubtitlesCallback;
        _cancelSubtitlesCallback = cancelSubtitlesCallback;

        _status = model.Status;
        _progress = model.Progress;
        _errorMessage = model.Error?.Message;
        _subtitlesErrorMessage = model.SubtitlesError?.Message;
    }

    #region --- NOT EDITABLE BY USER ---

    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [ObservableProperty] private DownloadStatus _status;

    [ObservableProperty] private double _progress;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _subtitlesErrorMessage;
    [ObservableProperty] private bool _areSubtitlesLoading;

    [RelayCommand]
    private void Proceed() => _proceedCallback(Model.Id);

    [RelayCommand]
    private void Cancel() => _cancelCallback(Model.Id);

    [RelayCommand]
    private void FetchSubtitles() => _fetchSubtitlesCallback(Model.Id);

    [RelayCommand]
    private void CancelSubtitles() => _cancelSubtitlesCallback(Model.Id);

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

    [ObservableProperty] private TabItemViewModel[]? _subtitleTabs;

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

    public virtual void Refresh()
    {
        Metadata = Model.Metadata;

        Status = Model.Status;
        Progress = Model.Progress;
        ErrorMessage = Model.Error?.Message;

        SubtitlesErrorMessage = Model.SubtitlesError?.Message;
        AreSubtitlesLoading = Model.AreSubtitlesLoading;

        // Assign only if tabs were not initialized and if subtitles are not null
        if (SubtitleTabs is null && Model.Subtitles is not null)
            SubtitleTabs = [.. Model.Subtitles.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];

        // Setting `CustomTitle` as `Title` if user hasn't typed anything yet
        if (CanEdit && string.IsNullOrWhiteSpace(CustomTitle))
            CustomTitle = Model.Metadata?.Title;
    }
}

public record TabItemViewModel(string Header, string Content);
