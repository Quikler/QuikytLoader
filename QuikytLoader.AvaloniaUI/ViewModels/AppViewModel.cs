using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.AvaloniaUI.Services;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class AppViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    [NotifyPropertyChangedFor(nameof(IsHomeSelected))]
    [NotifyPropertyChangedFor(nameof(IsSettingsSelected))]
    [NotifyCanExecuteChangedFor(nameof(NavigateToHomeCommand))]
    [NotifyCanExecuteChangedFor(nameof(NavigateToSettingsCommand))]
    [ObservableProperty] private ViewModelBase _currentView;

    private bool CanNavigateToHome => !IsHomeSelected;
    private bool CanNavigateToSettings => !IsSettingsSelected;

    public bool IsHomeSelected => CurrentView == HomeViewModel;
    public bool IsSettingsSelected => CurrentView == SettingsViewModel;

    public HomeViewModel HomeViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public AppViewModel(
        HomeViewModel homeViewModel,
        SettingsViewModel settingsViewModel,
        IDialogService dialogService,
        IThemeApplier themeApplier)
    {
        HomeViewModel = homeViewModel;
        SettingsViewModel = settingsViewModel;
        _dialogService = dialogService;

        _currentView = HomeViewModel;

        SettingsViewModel.LoadSettings();
        themeApplier.ApplyFromSettings();
    }

    [RelayCommand(CanExecute = nameof(CanNavigateToHome))]
    private async Task NavigateToHomeAsync()
    {
        if (SettingsViewModel.HasUnsavedChanges())
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Unsaved Changes",
                "You have unsaved changes that will be lost. Continue?");

            if (!confirmed) return;
        }

        CurrentView = HomeViewModel;
    }

    [RelayCommand(CanExecute = nameof(CanNavigateToSettings))]
    private void NavigateToSettings()
    {
        SettingsViewModel.LoadSettings();
        CurrentView = SettingsViewModel;
    }
}
