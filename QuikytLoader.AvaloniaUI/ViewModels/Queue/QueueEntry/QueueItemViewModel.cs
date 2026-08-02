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

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

public partial class QueueItemViewModel : QueueEntryViewModel
{
    protected QueueItem Model { get; }

    private readonly Action<Guid> _proceedCallback;
    private readonly Action<Guid> _cancelCallback;

    private readonly FetchManualSubtitlesUseCase _fetchManualSubtitlesUseCase;
    private readonly FetchAutoSubtitlesUseCase _fetchAutoSubtitlesUseCase;
    private readonly CancelSubtitlesUseCase _cancelSubtitlesUseCase;

    private bool _autoSubtitlesOptionWasSavedToSettings;

    public SettingsViewModel SettingsViewModel { get; }

    public QueueItemViewModel(
        QueueItem model,
        Action<Guid> proceedCallback,
        Action<Guid> cancelCallback,
        FetchManualSubtitlesUseCase fetchManualSubtitlesUseCase,
        FetchAutoSubtitlesUseCase fetchAutoSubtitlesUseCase,
        CancelSubtitlesUseCase cancelSubtitlesUseCase,
        SettingsViewModel settingsViewModel)
    {
        Model = model;

        _proceedCallback = proceedCallback;
        _cancelCallback = cancelCallback;

        _fetchManualSubtitlesUseCase = fetchManualSubtitlesUseCase;
        _fetchAutoSubtitlesUseCase = fetchAutoSubtitlesUseCase;
        _cancelSubtitlesUseCase = cancelSubtitlesUseCase;

        SettingsViewModel = settingsViewModel;
        SettingsViewModel.AutoSubtitlesOptionWasSavedToSettings += autoSubtitlesOptionWasSavedToSettings =>
        {
            if (!autoSubtitlesOptionWasSavedToSettings ||
                SubtitleState is SubtitleIdleState
                    or SubtitleSuccessState
                    or SubtitleErrorState { AllowRetry: false }) return;

            if (SubtitleState is SubtitleLoadingState)
            {
                CancelSubtitles();
                _autoSubtitlesOptionWasSavedToSettings = true;
            }
            else
            {
                SubtitleState = new SubtitleAutoSubtitlesOptionSettingsChangedState(
                    "Auto Subtitles Option settings were changed, please click refresh");
            }
        };

        RefreshInternal();
    }

    #region --- NOT EDITABLE BY USER ---

    [NotifyPropertyChangedFor(nameof(StatusMessage))]
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyPropertyChangedFor(nameof(CanEdit))]
    [ObservableProperty] private DownloadStatus _status;

    [ObservableProperty] private double _progress;
    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private Language _selectedAutoSubtitlesLanguage = Language.English;
    [ObservableProperty] private SubtitleUiState _subtitleState = new SubtitleIdleState();
    [ObservableProperty] private TabItemViewModel[]? _subtitleTabs;
    [ObservableProperty] private TabItemViewModel? _selectedTab;

    [ObservableProperty] private bool _areSubtitlesLoading;
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

            if (SubtitleState is SubtitleIdleState)
                _ = FetchSubtitlesWorkflow();
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

    [RelayCommand]
    private Task FetchSubtitlesWorkflow() => FetchSubtitlesWorkflowInternal(null);

    [RelayCommand]
    private Task FetchSubtitlesWithLanguageWorkflow() => FetchSubtitlesWorkflowInternal(SelectedAutoSubtitlesLanguage);

    private async Task FetchSubtitlesWorkflowInternal(Language? language)
    {
        var manualResult = await FetchManualSubtitles();

        switch (manualResult)
        {
            case SubtitleFetchResult.Failed:
            case SubtitleFetchResult.Canceled:
                return;

            case SubtitleFetchResult.Fetched:
            case SubtitleFetchResult.NotFound:
            case SubtitleFetchResult.NotAllowed:
                break;
        }

        await FetchAutoSubtitles(language);
    }

    private async Task<SubtitleFetchResult> FetchManualSubtitles()
    {
        SubtitleState = new SubtitleLoadingState();

        var result = await _fetchManualSubtitlesUseCase.ExecuteAsync(Model.Id);

        switch (result)
        {
            case SubtitleFetchResult.Fetched:
                SubtitleTabs = [.. Model.Subtitles!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];
                SubtitleState = new SubtitleSuccessState();
                break;

            case SubtitleFetchResult.Failed r:
                SubtitleState = new SubtitleErrorState(r.Message, r.AllowRetry, r.DetailsMessage);
                break;

            case SubtitleFetchResult.NotFound r:
                SubtitleState = new SubtitleErrorState(r.Message, r.AllowRetry, null);
                break;

            case SubtitleFetchResult.Canceled r:
                if (_autoSubtitlesOptionWasSavedToSettings)
                {
                    SubtitleState = new SubtitleAutoSubtitlesOptionSettingsChangedState(
                        "Auto Subtitles Option settings were changed, please click refresh");
                    _autoSubtitlesOptionWasSavedToSettings = false;
                    break;
                }
                SubtitleState = new SubtitleErrorState(r.Message, r.AllowRetry, null);
                break;

            case SubtitleFetchResult.NotAllowed r:
                SubtitleState = SubtitleState;
                break;
        }

        return result;
    }

    private async Task<SubtitleFetchResult> FetchAutoSubtitles(Language? language)
    {
        SubtitleState = new SubtitleLoadingState();

        var result = await _fetchAutoSubtitlesUseCase.ExecuteAsync(
            Model.Id,
            language);

        switch (result)
        {
            case SubtitleFetchResult.Fetched:
                SubtitleTabs = [.. Model.Subtitles!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];
                SubtitleState = new SubtitleSuccessState();
                break;

            case SubtitleFetchResult.ActionRequired r:
                if (_autoSubtitlesOptionWasSavedToSettings)
                {
                    SubtitleState = new SubtitleAutoSubtitlesOptionSettingsChangedState(
                        "Auto Subtitles Option settings were changed, please click refresh");
                    _autoSubtitlesOptionWasSavedToSettings = false;
                    break;
                }

                SubtitleState = r.SubtitleActionRequired switch
                {
                    SubtitleActionRequired.ChangeAutoSubtitlesOption =>
                        new SubtitleChangeAutoSubtitlesOptionState(r.Message, r.DetailsMessage),
                    SubtitleActionRequired.LanguageSelection => r.IsError
                        ? new SubtitleRetryLanguageSelectionState(r.Message, r.DetailsMessage)
                        : new SubtitleLanguageSelectionState(r.Message, r.DetailsMessage),
                    _ => throw new InvalidOperationException()
                };
                break;

            case SubtitleFetchResult.Failed r:
                SubtitleState = new SubtitleErrorState(r.Message, r.AllowRetry, r.DetailsMessage);
                break;

            case SubtitleFetchResult.NotFound r:
                SubtitleState = new SubtitleErrorState(r.Message, r.AllowRetry, null);
                break;

            case SubtitleFetchResult.Canceled r:
                if (_autoSubtitlesOptionWasSavedToSettings)
                {
                    SubtitleState = new SubtitleAutoSubtitlesOptionSettingsChangedState(
                        "Auto Subtitles Option settings were changed, please click refresh");
                    _autoSubtitlesOptionWasSavedToSettings = false;
                    break;
                }
                SubtitleState = new SubtitleErrorState(r.Message, r.AllowRetry, null);
                break;

            case SubtitleFetchResult.NotAllowed:
                SubtitleState = SubtitleState;
                break;
        }

        return result;
    }

    [RelayCommand]
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

public record TabItemViewModel(string Header, string Content);

public abstract record SubtitleUiState;

public sealed record SubtitleIdleState : SubtitleUiState;
public sealed record SubtitleLoadingState : SubtitleUiState;
public sealed record SubtitleErrorState(string Message, bool AllowRetry, string? DetailsMessage) : SubtitleUiState;
public sealed record SubtitleSuccessState : SubtitleUiState;
public sealed record SubtitleLanguageSelectionState(string Message, string? DetailsMessage) : SubtitleUiState;
public sealed record SubtitleRetryLanguageSelectionState(string Message, string? DetailsMessage) : SubtitleUiState;
public sealed record SubtitleChangeAutoSubtitlesOptionState(string Message, string? DetailsMessage) : SubtitleUiState;
public sealed record SubtitleAutoSubtitlesOptionSettingsChangedState(string Message) : SubtitleUiState;
