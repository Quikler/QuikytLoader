using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;
using System.Threading.Tasks;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class SettingsViewModel(ManageSettingsUseCase manageSettingsUseCase) : ViewModelBase
{
    [ObservableProperty]
    private string _botToken = string.Empty;

    [ObservableProperty]
    private string _chatId = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    private string _savedBotToken = string.Empty;
    private string _savedChatId = string.Empty;

    public bool HasUnsavedChanges =>
        BotToken != _savedBotToken || ChatId != _savedChatId;

    public async Task InitializeAsync()
    {
        var settings = await manageSettingsUseCase.LoadSettingsAsync();
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;

        _savedBotToken = BotToken;
        _savedChatId = ChatId;
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

        _savedBotToken = BotToken;
        _savedChatId = ChatId;

        StatusMessage = "Settings saved successfully!";
    }
}
