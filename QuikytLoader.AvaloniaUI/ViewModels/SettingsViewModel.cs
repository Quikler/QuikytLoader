using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Models;
using QuikytLoader.Services;

namespace QuikytLoader.ViewModels;

/// <summary>
/// ViewModel for the Settings page
/// Handles Telegram bot configuration
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsManager _settingsManager;

    [ObservableProperty]
    private string _botToken = string.Empty;

    [ObservableProperty]
    private string _chatId = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        LoadSettings();
    }

    /// <summary>
    /// Loads settings from storage
    /// </summary>
    private void LoadSettings()
    {
        var settings = _settingsManager.Load();
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;
    }

    /// <summary>
    /// Saves settings to storage
    /// </summary>
    [RelayCommand]
    private void SaveSettings()
    {
        var settings = new AppSettings
        {
            BotToken = BotToken,
            ChatId = ChatId
        };

        _settingsManager.Save(settings);
        StatusMessage = "Settings saved successfully!";
    }
}
