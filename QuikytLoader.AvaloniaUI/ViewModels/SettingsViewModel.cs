using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;

namespace QuikytLoader.AvaloniaUI.ViewModels;

/// <summary>
/// ViewModel for the Settings page (Telegram bot configuration)
/// Uses Application layer Use Cases to manage settings
/// </summary>
public partial class SettingsViewModel(ManageSettingsUseCase manageSettingsUseCase) : ViewModelBase
{
    [ObservableProperty]
    private string _botToken = string.Empty;

    [ObservableProperty]
    private string _chatId = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Loads settings from storage on initialization
    /// </summary>
    public void Initialize()
    {
        LoadSettings();
    }

    /// <summary>
    /// Loads settings from storage
    /// </summary>
    private void LoadSettings()
    {
        var settings = manageSettingsUseCase.LoadSettings();
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;
    }

    /// <summary>
    /// Saves settings to storage
    /// </summary>
    [RelayCommand]
    private void SaveSettings()
    {
        var settings = new AppSettingsDto
        {
            BotToken = BotToken,
            ChatId = ChatId
        };

        manageSettingsUseCase.SaveSettings(settings);
        StatusMessage = "Settings saved successfully!";
    }
}
