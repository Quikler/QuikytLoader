using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace QuikytLoader.ViewModels;

/// <summary>
/// ViewModel for the main application window
/// Handles YouTube URL input and download/send operations
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _youtubeUrl = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private double _progressValue = 0;

    [ObservableProperty]
    private bool _isProgressVisible = false;

    // Services will be injected here later
    // private readonly IYouTubeDownloadService _youtubeService;
    // private readonly ITelegramBotService _telegramService;

    public MainWindowViewModel()
    {
        // Constructor - services will be injected via DI
    }

    /// <summary>
    /// Command to download YouTube video and send to Telegram
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteDownload))]
    private async Task DownloadAndSendAsync()
    {
        if (!ValidateUrl())
        {
            UpdateStatus("Invalid YouTube URL");
            return;
        }

        await ProcessDownloadAndSendAsync();
    }

    /// <summary>
    /// Determines if the download command can execute
    /// </summary>
    private bool CanExecuteDownload()
    {
        return !IsProcessing && HasValidUrl();
    }

    /// <summary>
    /// Validates if the URL is not empty and has basic YouTube format
    /// </summary>
    private bool ValidateUrl()
    {
        if (string.IsNullOrWhiteSpace(YoutubeUrl))
        {
            return false;
        }

        return IsYouTubeUrl(YoutubeUrl);
    }

    /// <summary>
    /// Checks if URL contains YouTube domain
    /// </summary>
    private static bool IsYouTubeUrl(string url)
    {
        return url.Contains("youtube.com", System.StringComparison.OrdinalIgnoreCase) ||
               url.Contains("youtu.be", System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if current URL has valid format
    /// </summary>
    private bool HasValidUrl()
    {
        return !string.IsNullOrWhiteSpace(YoutubeUrl) && IsYouTubeUrl(YoutubeUrl);
    }

    /// <summary>
    /// Main workflow: Download from YouTube and send to Telegram
    /// </summary>
    private async Task ProcessDownloadAndSendAsync()
    {
        SetProcessingState(true);
        ShowProgress();

        try
        {
            await DownloadFromYouTubeAsync();
            await SendToTelegramAsync();

            HandleSuccess();
        }
        catch (System.Exception ex)
        {
            HandleError(ex.Message);
        }
        finally
        {
            SetProcessingState(false);
            HideProgress();
        }
    }

    /// <summary>
    /// Downloads video from YouTube and converts to MP3
    /// </summary>
    private async Task DownloadFromYouTubeAsync()
    {
        UpdateStatus("Downloading from YouTube...");
        UpdateProgress(30);

        // TODO: Call YouTube download service
        await Task.Delay(1000); // Simulated delay

        UpdateProgress(60);
    }

    /// <summary>
    /// Sends the downloaded file to Telegram bot
    /// </summary>
    private async Task SendToTelegramAsync()
    {
        UpdateStatus("Sending to Telegram...");
        UpdateProgress(80);

        // TODO: Call Telegram bot service
        await Task.Delay(1000); // Simulated delay

        UpdateProgress(100);
    }

    /// <summary>
    /// Handles successful completion of the workflow
    /// </summary>
    private void HandleSuccess()
    {
        UpdateStatus("✓ Successfully sent to Telegram!");
        ClearUrl();
    }

    /// <summary>
    /// Handles errors during the workflow
    /// </summary>
    private void HandleError(string errorMessage)
    {
        UpdateStatus($"Error: {errorMessage}");
        ResetProgress();
    }

    /// <summary>
    /// Sets the processing state and updates command availability
    /// </summary>
    private void SetProcessingState(bool isProcessing)
    {
        IsProcessing = isProcessing;
        DownloadAndSendCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Updates the status message displayed to the user
    /// </summary>
    private void UpdateStatus(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// Updates the progress bar value
    /// </summary>
    private void UpdateProgress(double value)
    {
        ProgressValue = value;
    }

    /// <summary>
    /// Shows the progress bar
    /// </summary>
    private void ShowProgress()
    {
        IsProgressVisible = true;
        ResetProgress();
    }

    /// <summary>
    /// Hides the progress bar
    /// </summary>
    private void HideProgress()
    {
        IsProgressVisible = false;
    }

    /// <summary>
    /// Resets progress to zero
    /// </summary>
    private void ResetProgress()
    {
        ProgressValue = 0;
    }

    /// <summary>
    /// Clears the URL input field
    /// </summary>
    private void ClearUrl()
    {
        YoutubeUrl = string.Empty;
    }

    /// <summary>
    /// Called when YoutubeUrl property changes
    /// Updates command availability based on URL validity
    /// </summary>
    partial void OnYoutubeUrlChanged(string value)
    {
        DownloadAndSendCommand.NotifyCanExecuteChanged();
    }
}
