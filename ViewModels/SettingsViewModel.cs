using CommunityToolkit.Mvvm.ComponentModel;

namespace QuikytLoader.ViewModels;

/// <summary>
/// ViewModel for the Settings page
/// Handles Telegram bot configuration
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _botToken = string.Empty;

    [ObservableProperty]
    private string _chatId = string.Empty;

    public SettingsViewModel()
    {
        LoadSettings();
    }

    /// <summary>
    /// Loads settings from storage
    /// </summary>
    private void LoadSettings()
    {
        // TODO: Load from persistent storage
    }

    /// <summary>
    /// Saves settings to storage
    /// </summary>
    public void SaveSettings()
    {
        // TODO: Save to persistent storage
    }
}
