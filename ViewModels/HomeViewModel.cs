using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Models;
using QuikytLoader.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QuikytLoader.ViewModels;

/// <summary>
/// ViewModel for the Home page (YouTube download functionality)
/// </summary>
public partial class HomeViewModel(IYouTubeDownloadService youtubeService, ITelegramBotService telegramService) : ViewModelBase
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

    private DownloadResult? _downloadResult;
    private CancellationTokenSource? _cancellationTokenSource;

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
    /// Command to cancel the ongoing download
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteCancel))]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        UpdateStatus("Cancelling download...");
    }

    /// <summary>
    /// Determines if the cancel command can execute
    /// </summary>
    private bool CanExecuteCancel()
    {
        return IsProcessing && _cancellationTokenSource != null;
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
        return url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
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
        // Create new cancellation token source for this download
        _cancellationTokenSource = new CancellationTokenSource();
        SetProcessingState(true);
        ShowProgress();

        try
        {
            await DownloadFromYouTubeAsync();
            await SendToTelegramAsync();
            // Future services can use thumbnail here before cleanup

            HandleSuccess();
        }
        catch (OperationCanceledException)
        {
            HandleCancellation();
        }
        catch (Exception ex)
        {
            HandleError(ex.Message);
        }
        finally
        {
            // Cleanup temp files after all workflow steps complete
            CleanupTempFiles();

            // Dispose and clear the cancellation token source
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            SetProcessingState(false);
            HideProgress();
        }
    }

    /// <summary>
    /// Downloads video from YouTube and converts to MP3 with thumbnail
    /// </summary>
    private async Task DownloadFromYouTubeAsync()
    {
        UpdateStatus("Downloading from YouTube...");

        var progress = new Progress<double>(UpdateProgress);
        _downloadResult = await youtubeService.DownloadAsync(YoutubeUrl, progress, _cancellationTokenSource!.Token);
    }

    /// <summary>
    /// Sends the downloaded file to Telegram bot with thumbnail
    /// </summary>
    private async Task SendToTelegramAsync()
    {
        UpdateStatus("Sending to Telegram...");
        UpdateProgress(Random.Shared.Next(50, 80));

        if (_downloadResult is null)
        {
            throw new InvalidOperationException("No file to send. Download failed.");
        }

        await telegramService.SendAudioAsync(_downloadResult.AudioFilePath, _downloadResult.ThumbnailPath);
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
    /// Handles cancellation of the workflow
    /// </summary>
    private void HandleCancellation()
    {
        UpdateStatus("Download cancelled");
        ResetProgress();
    }

    /// <summary>
    /// Sets the processing state and updates command availability
    /// </summary>
    private void SetProcessingState(bool isProcessing)
    {
        IsProcessing = isProcessing;
        DownloadAndSendCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
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

    /// <summary>
    /// Cleans up temporary files created during the download workflow
    /// Deletes the temporary thumbnail file if it exists
    /// Called after all workflow steps complete (success or failure)
    /// </summary>
    private void CleanupTempFiles()
    {
        if (_downloadResult?.ThumbnailPath == null)
        {
            return;
        }

        try
        {
            if (File.Exists(_downloadResult.ThumbnailPath))
            {
                File.Delete(_downloadResult.ThumbnailPath);
                Console.WriteLine($"Cleaned up temp thumbnail: {_downloadResult.ThumbnailPath}");
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the workflow if cleanup fails
            Console.WriteLine($"Failed to cleanup temp thumbnail: {ex.Message}");
        }
    }
}
