using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
    [ObservableProperty] private TabItemViewModel? _selectedTab;

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
                SubtitlesTabs = [.. Model.Dictionary!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];
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
                SubtitlesTabs = [.. Model.Dictionary!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];
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

public record TabItemViewModel(string Header, string Content);

public abstract record SubtitlesUiState;
public sealed record SubtitlesIdleState : SubtitlesUiState;
public sealed record SubtitlesLoadingState(string LoadingMessage) : SubtitlesUiState;
public sealed record SubtitlesErrorState(string Message, bool AllowRetry, string? DetailsMessage, bool DisplayCloseButton = false) : SubtitlesUiState;
public sealed record SubtitlesSuccessState : SubtitlesUiState;
public sealed record SubtitlesLanguageSelectionState(string Message, string? DetailsMessage, bool DisplayCloseButton) : SubtitlesUiState;
public sealed record SubtitlesRetryLanguageSelectionState(string Message, string? DetailsMessage, bool DisplayCloseButton) : SubtitlesUiState;
public sealed record SubtitlesAutoSubtitlesOptionSettingsChangedState(string Message, bool DisplayCloseButton) : SubtitlesUiState;
