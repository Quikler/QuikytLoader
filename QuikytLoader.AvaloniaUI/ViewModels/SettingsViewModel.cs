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

    public async Task InitializeAsync() => await LoadSettingsAsync();

    private async Task LoadSettingsAsync()
    {
        var settings = await manageSettingsUseCase.LoadSettingsAsync();
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;
    }

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
