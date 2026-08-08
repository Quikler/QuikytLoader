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

    private readonly FetchManualSubtitlesUseCase _fetchManualSubtitlesUseCase;
    private readonly FetchAutoSubtitlesUseCase _fetchAutoSubtitlesUseCase;
    private readonly CancelSubtitlesUseCase _cancelSubtitlesUseCase;

    public SettingsViewModel SettingsViewModel { get; }

    public QueueItemSubtitlesViewModel(
        Domain.Entities.Subtitles model,
        IUserSettings userSettings,
        FetchManualSubtitlesUseCase fetchManualSubtitlesUseCase,
        FetchAutoSubtitlesUseCase fetchAutoSubtitlesUseCase,
        CancelSubtitlesUseCase cancelSubtitlesUseCase,
        SettingsViewModel settingsViewModel)
    {
        Model = model;

        userSettings.Changed += args =>
        {
            if (args.OldSettings.AutoSubtitlesOption == args.NewSettings.AutoSubtitlesOption
                || SubtitleState is SubtitleIdleState
                    or SubtitleSuccessState
                    or SubtitleErrorState { AllowRetry: false })
                return;

            SubtitleState = new SubtitleAutoSubtitlesOptionSettingsChangedState(
                "Auto Subtitles Option settings were changed, please click refresh", Model.AreAutoSubtitlesLoaded);
        };

        _fetchManualSubtitlesUseCase = fetchManualSubtitlesUseCase;
        _fetchAutoSubtitlesUseCase = fetchAutoSubtitlesUseCase;
        _cancelSubtitlesUseCase = cancelSubtitlesUseCase;

        SettingsViewModel = settingsViewModel;
    }

    [ObservableProperty] private Language _selectedAutoSubtitlesLanguage = Language.English;
    [ObservableProperty] private SubtitleUiState _subtitleState = new SubtitleIdleState();
    [ObservableProperty] private TabItemViewModel[]? _subtitleTabs;
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
        SubtitleState = new SubtitleLoadingState("Loading manual subtitles...");

        var result = await _fetchManualSubtitlesUseCase.ExecuteAsync(
            Model.QueueItemId);

        switch (result)
        {
            case SubtitleFetchResult.Fetched:
                SubtitleTabs = [.. Model.Dictionary!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];
                SubtitleState = new SubtitleSuccessState();
                break;

            case SubtitleFetchResult.Failed r:
                SubtitleState = new SubtitleErrorState(r.Message, Model.AllowManualSubtitlesLoading, r.DetailsMessage);
                break;

            case SubtitleFetchResult.NotFound r:
                SubtitleState = new SubtitleErrorState(r.Message, Model.AllowManualSubtitlesLoading, null);
                break;

            case SubtitleFetchResult.Canceled r:
                SubtitleState = new SubtitleErrorState(r.Message, Model.AllowManualSubtitlesLoading, null);
                break;

            case SubtitleFetchResult.NotAllowed:
                SubtitleState = SubtitleState;
                break;
        }

        return result;
    }

    private async Task<SubtitleFetchResult> FetchAutoSubtitles(Language? language)
    {
        SubtitleState = new SubtitleLoadingState("Loading auto subtitles...");

        var result = await _fetchAutoSubtitlesUseCase.ExecuteAsync(
            Model.QueueItemId,
            language);

        switch (result)
        {
            case SubtitleFetchResult.Fetched r:
                SubtitleTabs = [.. Model.Dictionary!.Select(kvp => new TabItemViewModel(kvp.Key, kvp.Value))];
                if (r.Action is null)
                {
                    SubtitleState = new SubtitleSuccessState();
                    break;
                }

                SubtitleState = r.Action.SubtitleActionRequired switch
                {
                    SubtitleActionRequired.LanguageSelection =>
                        new SubtitleLanguageSelectionState(r.Action.Message, null, Model.AreAutoSubtitlesLoaded),
                    _ => throw new UnreachableException()
                };
                break;

            case SubtitleFetchResult.ActionRequired r:
                SubtitleState = r.SubtitleActionRequired switch
                {
                    SubtitleActionRequired.ChangeAutoSubtitlesOption =>
                        new SubtitleChangeAutoSubtitlesOptionState(r.Message, r.DetailsMessage, Model.AreAutoSubtitlesLoaded),
                    SubtitleActionRequired.LanguageSelection => r.IsError
                        ? new SubtitleRetryLanguageSelectionState(r.Message, r.DetailsMessage, Model.AreAutoSubtitlesLoaded)
                        : new SubtitleLanguageSelectionState(r.Message, r.DetailsMessage, Model.AreAutoSubtitlesLoaded),
                    SubtitleActionRequired.RefreshDueToSettingsChange =>
                        new SubtitleAutoSubtitlesOptionSettingsChangedState(r.Message, Model.AreAutoSubtitlesLoaded),
                    _ => throw new UnreachableException()
                };
                break;

            case SubtitleFetchResult.Failed r:
                SubtitleState = new SubtitleErrorState(r.Message, Model.AllowAutoSubtitlesLoading, r.DetailsMessage, Model.AreAutoSubtitlesLoaded);
                break;

            case SubtitleFetchResult.NotFound r:
                SubtitleState = new SubtitleErrorState(r.Message, Model.AllowAutoSubtitlesLoading, null, Model.AreAutoSubtitlesLoaded);
                break;

            case SubtitleFetchResult.Canceled r:
                SubtitleState = new SubtitleErrorState(r.Message, Model.AllowAutoSubtitlesLoading, null, Model.AreAutoSubtitlesLoaded);
                break;

            case SubtitleFetchResult.NotAllowed:
                SubtitleState = SubtitleState;
                break;
        }

        return result;
    }

    [RelayCommand]
    private void CancelSubtitles() => _cancelSubtitlesUseCase.Execute(Model.QueueItemId);
}

public record TabItemViewModel(string Header, string Content);

public abstract record SubtitleUiState;
public sealed record SubtitleIdleState : SubtitleUiState;
public sealed record SubtitleLoadingState(string LoadingMessage) : SubtitleUiState;
public sealed record SubtitleErrorState(string Message, bool AllowRetry, string? DetailsMessage, bool DisplayCloseButton = false) : SubtitleUiState;
public sealed record SubtitleSuccessState : SubtitleUiState;
public sealed record SubtitleLanguageSelectionState(string Message, string? DetailsMessage, bool DisplayCloseButton) : SubtitleUiState;
public sealed record SubtitleRetryLanguageSelectionState(string Message, string? DetailsMessage, bool DisplayCloseButton) : SubtitleUiState;
public sealed record SubtitleChangeAutoSubtitlesOptionState(string Message, string? DetailsMessage, bool DisplayCloseButton) : SubtitleUiState;
public sealed record SubtitleAutoSubtitlesOptionSettingsChangedState(string Message, bool DisplayCloseButton) : SubtitleUiState;
