using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;
using System.Threading.Tasks;

namespace QuikytLoader.AvaloniaUI.ViewModels;

/// <summary>
/// ViewModel for the Settings page (Telegram bot configuration)
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
    /// Loads settings from disk. Called on every navigation to Settings page
    /// to ensure UI reflects persisted state (discards any unsaved edits).
    /// TODO: Consider adding "Discard unsaved changes?" confirmation dialog
    /// to prevent user confusion where they might mistake unsaved edits for persisted settings.
    /// </summary>
    public async Task InitializeAsync()
    {
        var settings = await manageSettingsUseCase.LoadSettingsAsync();
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        await manageSettingsUseCase.SaveSettingsAsync(
            new AppSettingsDto
            {
                BotToken = BotToken,
                ChatId = ChatId
            });

        StatusMessage = "Settings saved successfully!";
    }
}
