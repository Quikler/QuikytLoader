using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.AvaloniaUI.Services;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class AppViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private bool _isHomeSelected = true;

    [ObservableProperty]
    private bool _isSettingsSelected = false;

    public HomeViewModel HomeViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public AppViewModel(
        HomeViewModel homeViewModel,
        SettingsViewModel settingsViewModel,
        IDialogService dialogService)
    {
        HomeViewModel = homeViewModel;
        SettingsViewModel = settingsViewModel;
        _dialogService = dialogService;

        _currentView = HomeViewModel;
    }

    [RelayCommand]
    private async Task NavigateToHomeAsync()
    {
        if (CurrentView == SettingsViewModel && SettingsViewModel.HasUnsavedChanges)
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Unsaved Changes",
                "You have unsaved changes that will be lost. Continue?");

            if (!confirmed) return;
        }

        CurrentView = HomeViewModel;
        IsHomeSelected = true;
        IsSettingsSelected = false;
    }

    [RelayCommand]
    private async Task NavigateToSettingsAsync()
    {
        await SettingsViewModel.InitializeAsync();
        CurrentView = SettingsViewModel;
        IsHomeSelected = false;
        IsSettingsSelected = true;
    }
}
