using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;
using System.Threading.Tasks;

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
    public async Task InitializeAsync()
    {
        await LoadSettingsAsync();
    }

    /// <summary>
    /// Loads settings from storage asynchronously
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        var settings = await manageSettingsUseCase.LoadSettingsAsync();
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;
    }

    /// <summary>
    /// Saves settings to storage asynchronously
    /// </summary>
    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var settings = new AppSettingsDto
        {
            BotToken = BotToken,
            ChatId = ChatId
        };

        await manageSettingsUseCase.SaveSettingsAsync(settings);
        StatusMessage = "Settings saved successfully!";
    }
}
