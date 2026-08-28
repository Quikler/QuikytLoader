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

    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [ObservableProperty] private ThemePreference _themePreference;

    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [ObservableProperty] private string _botToken = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
    [ObservableProperty] private string _chatId = string.Empty;

    public bool HasUnsavedChanges() =>
        AutoSubtitlesOption != _userSettings.Current.AutoSubtitlesOption ||
        ThemePreference != _userSettings.Current.ThemePreference ||
        BotToken != _userSettings.Current.BotToken ||
        ChatId != _userSettings.Current.ChatId;

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

        SaveSettingsCommand.NotifyCanExecuteChanged();
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

        SaveSettingsCommand.NotifyCanExecuteChanged();
        StatusMessage = "Settings saved successfully!";
    }
}
