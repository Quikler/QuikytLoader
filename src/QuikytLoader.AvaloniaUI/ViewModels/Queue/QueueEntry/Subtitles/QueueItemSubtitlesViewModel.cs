using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.UseCases;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry.Subtitles;

public partial class QueueItemSubtitlesViewModel : ObservableObject
{
    private readonly Domain.Entities.Subtitles Model;

    private readonly IFetchManualSubtitlesUseCase _fetchManualSubtitlesUseCase;
    private readonly IFetchAutoSubtitlesUseCase _fetchAutoSubtitlesUseCase;
    private readonly ICancelSubtitlesUseCase _cancelSubtitlesUseCase;

    public event Action<int>? ScrollInSubtitles;

    public QueueItemSubtitlesViewModel(
        Domain.Entities.Subtitles model,
        IUserSettings userSettings,
        IFetchManualSubtitlesUseCase fetchManualSubtitlesUseCase,
        IFetchAutoSubtitlesUseCase fetchAutoSubtitlesUseCase,
        ICancelSubtitlesUseCase cancelSubtitlesUseCase)
    {
        Model = model;

        userSettings.Changed += args =>
        {
            if (args.OldSettings.AutoSubtitlesOption == args.NewSettings.AutoSubtitlesOption
                || SubtitlesState is SubtitlesIdleState
                    or SubtitlesSuccessState
                    or SubtitlesErrorState { AllowRetry: false })
                return;

            SubtitlesState = new SubtitlesAutoSubtitlesOptionSettingsChangedState(
                "Auto Subtitles Option settings were changed, please click refresh", Model.AreAutoSubtitlesLoaded);
        };

        _fetchManualSubtitlesUseCase = fetchManualSubtitlesUseCase;
        _fetchAutoSubtitlesUseCase = fetchAutoSubtitlesUseCase;
        _cancelSubtitlesUseCase = cancelSubtitlesUseCase;
    }

    [ObservableProperty] private Language _selectedAutoSubtitlesLanguage = Language.English;
    [ObservableProperty] private SubtitlesUiState _subtitlesState = new SubtitlesIdleState();
    [ObservableProperty] private TabItemViewModel[]? _subtitlesTabs;
    [NotifyPropertyChangedFor(nameof(IsSearchable))]
    [ObservableProperty] private TabItemViewModel? _selectedTab;

    [NotifyPropertyChangedFor(nameof(IsSearchable))]
    [ObservableProperty] private bool _areSubtitlesVisible;
    [ObservableProperty] private FASymbol _subtitlesIconSymbol = FASymbol.ClosedCaption;
    [ObservableProperty] private FASymbol _subtitlesChevronSymbol = FASymbol.ChevronDown;

    public bool IsSearchable => AreSubtitlesVisible && SelectedTab is not null;

    [RelayCommand]
    private void ToggleSubtitles()
    {
        AreSubtitlesVisible = !AreSubtitlesVisible;

        if (AreSubtitlesVisible)
        {
            SubtitlesIconSymbol = FASymbol.ClosedCaptionFilled;
            SubtitlesChevronSymbol = FASymbol.ChevronUp;

            if (SubtitlesState is SubtitlesIdleState)
                _ = FetchSubtitlesWorkflow();
        }
        else
        {
            SubtitlesIconSymbol = FASymbol.ClosedCaption;
            SubtitlesChevronSymbol = FASymbol.ChevronDown;
        }
    }

    [RelayCommand]
    private Task FetchSubtitlesWorkflow() => FetchSubtitlesWorkflowInternal(null);

    [RelayCommand]
    private Task FetchSubtitlesWithLanguageWorkflow() => FetchSubtitlesWorkflowInternal(SelectedAutoSubtitlesLanguage);

    private async Task FetchSubtitlesWorkflowInternal(Language? language)
    {
        var manualResult = await FetchManualSubtitles();

        switch (manualResult)
        {
            case SubtitlesFetchResult.Failed:
            case SubtitlesFetchResult.Canceled:
                return;

            case SubtitlesFetchResult.Fetched:
            case SubtitlesFetchResult.NotFound:
            case SubtitlesFetchResult.NotAllowed:
                break;
        }

        await FetchAutoSubtitles(language);
    }

    private async Task<SubtitlesFetchResult> FetchManualSubtitles()
    {
        SubtitlesState = new SubtitlesLoadingState("Loading manual subtitles...");

        var result = await _fetchManualSubtitlesUseCase.ExecuteAsync(
            Model.QueueItemId);

        switch (result)
        {
            case SubtitlesFetchResult.Fetched:
                SubtitlesTabs = [.. Model.Dictionary!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value, ScrollInSubtitles))];
                SubtitlesState = new SubtitlesSuccessState();
                break;

            case SubtitlesFetchResult.Failed r:
                SubtitlesState = new SubtitlesErrorState(r.Message, Model.AllowManualSubtitlesLoading, r.DetailsMessage);
                break;

            case SubtitlesFetchResult.NotFound r:
                SubtitlesState = new SubtitlesErrorState(r.Message, Model.AllowManualSubtitlesLoading, null);
                break;

            case SubtitlesFetchResult.Canceled r:
                SubtitlesState = new SubtitlesErrorState(r.Message, Model.AllowManualSubtitlesLoading, null);
                break;

            case SubtitlesFetchResult.NotAllowed:
                SubtitlesState = SubtitlesState;
                break;
        }

        return result;
    }

    private async Task<SubtitlesFetchResult> FetchAutoSubtitles(Language? language)
    {
        SubtitlesState = new SubtitlesLoadingState("Loading auto subtitles...");

        var result = await _fetchAutoSubtitlesUseCase.ExecuteAsync(
            Model.QueueItemId,
            language);

        switch (result)
        {
            case SubtitlesFetchResult.Fetched r:
                SubtitlesTabs = [.. Model.Dictionary!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value, ScrollInSubtitles))];
                if (r.Action is null)
                {
                    SubtitlesState = new SubtitlesSuccessState();
                    break;
                }

                SubtitlesState = r.Action.SubtitlesActionRequired switch
                {
                    SubtitlesActionRequired.LanguageSelection =>
                        new SubtitlesLanguageSelectionState(r.Action.Message, null, Model.AreAutoSubtitlesLoaded),
                    _ => throw new UnreachableException()
                };
                break;

            case SubtitlesFetchResult.ActionRequired r:
                SubtitlesState = r.SubtitlesActionRequired switch
                {
                    SubtitlesActionRequired.LanguageSelection => r.IsError
                        ? new SubtitlesRetryLanguageSelectionState(r.Message, r.DetailsMessage, Model.AreAutoSubtitlesLoaded)
                        : new SubtitlesLanguageSelectionState(r.Message, r.DetailsMessage, Model.AreAutoSubtitlesLoaded),
                    SubtitlesActionRequired.RefreshDueToSettingsChange =>
                        new SubtitlesAutoSubtitlesOptionSettingsChangedState(r.Message, Model.AreAutoSubtitlesLoaded),
                    _ => throw new UnreachableException()
                };
                break;

            case SubtitlesFetchResult.Failed r:
                SubtitlesState = new SubtitlesErrorState(r.Message, Model.AllowAutoSubtitlesLoading, r.DetailsMessage, Model.AreAutoSubtitlesLoaded);
                break;

            case SubtitlesFetchResult.NotFound r:
                SubtitlesState = new SubtitlesErrorState(r.Message, Model.AllowAutoSubtitlesLoading, null, Model.AreAutoSubtitlesLoaded);
                break;

            case SubtitlesFetchResult.Canceled r:
                SubtitlesState = new SubtitlesErrorState(r.Message, Model.AllowAutoSubtitlesLoading, null, Model.AreAutoSubtitlesLoaded);
                break;

            case SubtitlesFetchResult.NotAllowed:
                SubtitlesState = SubtitlesState;
                break;
        }

        return result;
    }

    [RelayCommand]
    private void CancelSubtitles() => _cancelSubtitlesUseCase.Execute(Model.QueueItemId);
}

public partial class TabItemViewModel(string header, string content, Action<int>? scrollInSubtitles) : ObservableObject
{
    public string Header => header;
    public string Content => content;

    private string? _findText;
    public string? FindText
    {
        get => _findText;
        set
        {
            _findText = value;
            Occurrences = string.IsNullOrEmpty(value)
                ? []
                : FindAllOccurrences(Content, value);

            GoToThePreviousOccurrence();

            static List<(int Start, int End)> FindAllOccurrences(string text, string search)
            {
                var occurrences = new List<(int Start, int End)>();
                var start = 0;

                while ((start = text.IndexOf(search, start)) >= 0)
                {
                    var end = start + search.Length;
                    occurrences.Add((start, end));
                    start = end;
                }

                return occurrences;
            }
        }
    }

    private List<(int Start, int End)> _occurrences = [];
    private List<(int Start, int End)> Occurrences
    {
        get => _occurrences;
        set
        {
            _occurrences = value;
            if (_occurrences.Count == 0)
            {
                CurrentOccurrenceIndex = -1;
                (SelectionStart, SelectionEnd) = (0, 0);
            }
            else
            {
                CurrentOccurrenceIndex = 0;
                (SelectionStart, SelectionEnd) = (Occurrences[CurrentOccurrenceIndex].Start, Occurrences[CurrentOccurrenceIndex].End);
            }
            OnPropertyChanged(nameof(OccurrencesCount));
        }
    }

    public int OccurrencesCount => Occurrences.Count;

    [ObservableProperty]
    private int _selectionStart;

    [ObservableProperty]
    private int _selectionEnd;

    [ObservableProperty]
    private int _currentOccurrenceIndex = -1;

    [RelayCommand]
    private void GoToTheNextOccurrence()
    {
        // Still perform a scroll when only one occurrence exists
        if (CurrentOccurrenceIndex + 1 >= OccurrencesCount)
        {
            scrollInSubtitles?.Invoke(SelectionStart);
            return;
        }

        CurrentOccurrenceIndex++;
        OnPropertyChanged(nameof(CurrentOccurrenceIndex));
        (SelectionStart, SelectionEnd) = (Occurrences[CurrentOccurrenceIndex].Start, Occurrences[CurrentOccurrenceIndex].End);
        scrollInSubtitles?.Invoke(SelectionStart);
    }

    [RelayCommand]
    private void GoToThePreviousOccurrence()
    {
        // Still perform a scroll when only one occurrence exists
        if (CurrentOccurrenceIndex - 1 < 0)
        {
            scrollInSubtitles?.Invoke(SelectionStart);
            return;
        }

        CurrentOccurrenceIndex--;
        OnPropertyChanged(nameof(CurrentOccurrenceIndex));
        (SelectionStart, SelectionEnd) = (Occurrences[CurrentOccurrenceIndex].Start, Occurrences[CurrentOccurrenceIndex].End);
        scrollInSubtitles?.Invoke(SelectionStart);
    }
}

public abstract record SubtitlesUiState;
public sealed record SubtitlesIdleState : SubtitlesUiState;
public sealed record SubtitlesLoadingState(string LoadingMessage) : SubtitlesUiState;
public sealed record SubtitlesErrorState(string Message, bool AllowRetry, string? DetailsMessage, bool DisplayCloseButton = false) : SubtitlesUiState;
public sealed record SubtitlesSuccessState : SubtitlesUiState;
public sealed record SubtitlesLanguageSelectionState(string Message, string? DetailsMessage, bool DisplayCloseButton) : SubtitlesUiState;
public sealed record SubtitlesRetryLanguageSelectionState(string Message, string? DetailsMessage, bool DisplayCloseButton) : SubtitlesUiState;
public sealed record SubtitlesAutoSubtitlesOptionSettingsChangedState(string Message, bool DisplayCloseButton) : SubtitlesUiState;
