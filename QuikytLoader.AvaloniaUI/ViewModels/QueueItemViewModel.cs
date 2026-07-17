using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using QuikytLoader.Application.UseCases;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class QueueItemViewModel : QueueEntryViewModel
{
    protected QueueItem Model { get; }

    private readonly Action<Guid> _proceedCallback;
    private readonly Action<Guid> _cancelCallback;

    private readonly FetchSubtitlesUseCase _fetchSubtitlesUseCase;
    private readonly CancelSubtitlesUseCase _cancelSubtitlesUseCase;

#pragma warning disable IDE0290 // Use primary constructor
    public QueueItemViewModel(
        QueueItem model,
        Action<Guid> proceedCallback,
        Action<Guid> cancelCallback,
        FetchSubtitlesUseCase fetchSubtitlesUseCase,
        CancelSubtitlesUseCase cancelSubtitlesUseCase)
#pragma warning restore IDE0290 // Use primary constructor
    {
        Model = model;
        _proceedCallback = proceedCallback;
        _cancelCallback = cancelCallback;
        _fetchSubtitlesUseCase = fetchSubtitlesUseCase;
        _cancelSubtitlesUseCase = cancelSubtitlesUseCase;

        _status = model.Status;
        _progress = model.Progress;
        _errorMessage = model.Error?.Message;
    }

    #region --- NOT EDITABLE BY USER ---

    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [ObservableProperty] private DownloadStatus _status;

    [ObservableProperty] private double _progress;
    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private TabItemViewModel[]? _subtitleTabs;
    [ObservableProperty] private string? _subtitlesErrorMessage;
    [ObservableProperty] private string? _autoSubtitlesMessage;

    [NotifyCanExecuteChangedFor(nameof(FetchSubtitlesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSubtitlesCommand))]
    [ObservableProperty] private bool _areSubtitlesLoading;

    [NotifyCanExecuteChangedFor(nameof(FetchSubtitlesCommand))]
    [ObservableProperty] private bool _allowSubtitlesLoading;

    private bool CanFetchSubtitles => AllowSubtitlesLoading && !AreSubtitlesLoading;
    private bool CanCancelSubtitles => AreSubtitlesLoading;

    [ObservableProperty] private bool _areSubtitlesVisible;
    [ObservableProperty] private FASymbol _subtitlesIconSymbol = FASymbol.ClosedCaption;
    [ObservableProperty] private FASymbol _subtitlesChevronSymbol = FASymbol.ChevronDown;

    [RelayCommand]
    private void ToggleSubtitles()
    {
        AreSubtitlesVisible = !AreSubtitlesVisible;
        if (AreSubtitlesVisible)
        {
            SubtitlesIconSymbol = FASymbol.ClosedCaptionFilled;
            SubtitlesChevronSymbol = FASymbol.ChevronUp;
            if (FetchSubtitlesCommand.CanExecute(null))
                FetchSubtitlesCommand.Execute(null);
        }
        else
        {
            SubtitlesIconSymbol = FASymbol.ClosedCaption;
            SubtitlesChevronSymbol = FASymbol.ChevronDown;
        }
    }

    [RelayCommand]
    private void Proceed() => _proceedCallback(Model.Id);

    [RelayCommand]
    private void Cancel() => _cancelCallback(Model.Id);

    [RelayCommand(CanExecute = nameof(CanFetchSubtitles))]
    private async Task FetchSubtitles(Language? language)
    {
        AutoSubtitlesMessage = null;

        var result = await _fetchSubtitlesUseCase.ExecuteAsync(QueueItemId, language?.Iso6391Name);
        switch (result)
        {
            case FetchSubtitlesResult.ManualLanguageSelectionRequired r:
                AutoSubtitlesMessage = r.Message;
                break;
            case FetchSubtitlesResult.ManuallySelectedLanguageMightBeWrong r:
                AutoSubtitlesMessage = r.Message;
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelSubtitles))]
    private void CancelSubtitles() => _cancelSubtitlesUseCase.Execute(Model.Id);

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

    public virtual void Refresh()
    {
        Metadata = Model.Metadata;

        Status = Model.Status;
        Progress = Model.Progress;
        ErrorMessage = Model.Error?.Message;

        SubtitlesErrorMessage = Model.SubtitlesError?.Message;
        AreSubtitlesLoading = Model.AreSubtitlesLoading;
        AllowSubtitlesLoading = Model.AllowSubtitlesLoading;

        // Assign only if tabs were not initialized and if subtitles are not null
        if (SubtitleTabs is null && Model.Subtitles is not null)
            SubtitleTabs = [.. Model.Subtitles.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];

        // Setting `CustomTitle` as `Title` if user hasn't typed anything yet
        if (CanEdit && string.IsNullOrWhiteSpace(CustomTitle))
            CustomTitle = Model.Metadata?.Title;
    }
}

public record TabItemViewModel(string Header, string Content);
