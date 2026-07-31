using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IUserSettings _userSettings;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [ObservableProperty] private AutoSubtitlesOption _autoSubtitlesOption;
    private AutoSubtitlesOption _savedAutoSubtitlesOption;

    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [ObservableProperty] private ThemePreference _themePreference;
    private ThemePreference _savedThemePreference;

    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [ObservableProperty] private string _botToken = string.Empty;
    private string _savedBotToken = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [ObservableProperty] private string _chatId = string.Empty;
    private string _savedChatId = string.Empty;

    public bool HasUnsavedChanges() =>
        AutoSubtitlesOption != _savedAutoSubtitlesOption ||
        ThemePreference != _savedThemePreference ||
        BotToken != _savedBotToken ||
        ChatId != _savedChatId;

    public event Action<bool>? AutoSubtitlesOptionWasSavedToSettings;

    public SettingsViewModel(IUserSettings userSettings)
    {
        _userSettings = userSettings;

        LoadSettings();
    }

    public void LoadSettings()
    {
        var settings = _userSettings.Current;

        AutoSubtitlesOption = settings.AutoSubtitlesOption;
        ThemePreference = settings.ThemePreference;
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;

        MarkAsSaved();
    }

    [RelayCommand(CanExecute = nameof(HasUnsavedChanges))]
    private void SaveSettings()
    {
        _userSettings.Current = new UserSettingsDto
        {
            AutoSubtitlesOption = AutoSubtitlesOption,
            ThemePreference = ThemePreference,
            ChatId = ChatId,
            BotToken = BotToken
        };

        AutoSubtitlesOptionWasSavedToSettings?.Invoke(AutoSubtitlesOption != _savedAutoSubtitlesOption);

        MarkAsSaved();
        StatusMessage = "Settings saved successfully!";
    }

    private void MarkAsSaved()
    {
        _savedAutoSubtitlesOption = AutoSubtitlesOption;
        _savedThemePreference = ThemePreference;
        _savedBotToken = BotToken;
        _savedChatId = ChatId;

        SaveSettingsCommand.NotifyCanExecuteChanged();
    }
}
