using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class SettingsViewModel(ManageSettingsUseCase manageSettingsUseCase, IThemeApplier themeApplier) : ViewModelBase
{
    public IThemeApplier ThemeApplier => themeApplier;

    public AutoSubtitlesOption[] AutoSubtitlesOptions { get; } = Enum.GetValues<AutoSubtitlesOption>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private AutoSubtitlesOption _autoSubtitlesOption;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private ThemePreference _themePreference;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _botToken = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsavedChanges))]
    private string _chatId = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    private AutoSubtitlesOption _savedAutoSubtitlesOption;
    private ThemePreference _savedThemePreference;
    private string _savedBotToken = string.Empty;
    private string _savedChatId = string.Empty;

    public bool HasUnsavedChanges =>
        AutoSubtitlesOption != _savedAutoSubtitlesOption ||
        ThemePreference != _savedThemePreference ||
        BotToken != _savedBotToken ||
        ChatId != _savedChatId;

    public void Initialize()
    {
        var settings = manageSettingsUseCase.LoadSettings();

        AutoSubtitlesOption = settings.AutoSubtitlesOption;
        ThemePreference = settings.ThemePreference;
        BotToken = settings.BotToken;
        ChatId = settings.ChatId;

        MarkAsSaved();
    }

    [RelayCommand]
    private void SaveSettings()
    {
        manageSettingsUseCase.SaveSettings(
            new UserSettingsDto
            {
                AutoSubtitlesOption = AutoSubtitlesOption,
                ThemePreference = ThemePreference,
                ChatId = ChatId,
                BotToken = BotToken
            });

        MarkAsSaved();
        StatusMessage = "Settings saved successfully!";
        themeApplier.Apply(ThemePreference);
    }

    private void MarkAsSaved()
    {
        _savedAutoSubtitlesOption = AutoSubtitlesOption;
        _savedThemePreference = ThemePreference;
        _savedBotToken = BotToken;
        _savedChatId = ChatId;
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }
}
