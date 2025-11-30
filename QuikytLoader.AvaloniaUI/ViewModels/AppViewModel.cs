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

    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentView = HomeViewModel;
        IsHomeSelected = true;
        IsSettingsSelected = false;
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentView = SettingsViewModel;
        IsHomeSelected = false;
        IsSettingsSelected = true;
    }
}
