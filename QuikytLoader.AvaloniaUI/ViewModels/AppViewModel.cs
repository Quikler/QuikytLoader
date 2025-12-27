using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QuikytLoader.AvaloniaUI.ViewModels;

/// <summary>
/// Root ViewModel for the application
/// Handles navigation between Home and Settings pages
/// </summary>
public partial class AppViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private bool _isHomeSelected = true;

    [ObservableProperty]
    private bool _isSettingsSelected = false;

    public HomeViewModel HomeViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public AppViewModel(HomeViewModel homeViewModel, SettingsViewModel settingsViewModel)
    {
        HomeViewModel = homeViewModel;
        SettingsViewModel = settingsViewModel;

        _currentView = HomeViewModel;
    }

    /// <summary>
    /// Switches the active view to the home view.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="HomeViewModel"/> as <see cref="CurrentView"/> and updates selection flags:
    /// sets <see cref="IsHomeSelected"/> to true and <see cref="IsSettingsSelected"/> to false.
    /// </remarks>
    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentView = HomeViewModel;
        IsHomeSelected = true;
        IsSettingsSelected = false;
    }

    /// <summary>
    /// Navigates the application to the Settings view, ensuring the Settings view model is initialized first.
    /// </summary>
    /// <remarks>
    /// Initializes <c>SettingsViewModel</c>, sets <c>CurrentView</c> to it, and updates the selection flags (<c>IsHomeSelected</c> and <c>IsSettingsSelected</c>).
    /// </remarks>
    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await SettingsViewModel.InitializeAsync();
        CurrentView = SettingsViewModel;
        IsHomeSelected = false;
        IsSettingsSelected = true;
    }
}