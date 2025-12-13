using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;
using QuikytLoader.Domain.ValueObjects;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuikytLoader.AvaloniaUI.ViewModels;

/// <summary>
/// ViewModel for the Home page (YouTube download functionality)
/// Uses Application layer Use Cases to orchestrate business logic
/// </summary>
public partial class HomeViewModel(
    DownloadAndSendUseCase downloadAndSendUseCase,
    CheckDuplicateUseCase checkDuplicateUseCase,
    GetVideoInfoUseCase getVideoInfoUseCase) : ViewModelBase
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

    [ObservableProperty]
    private bool _editTitle = false;

    [ObservableProperty]
    private string _customTitle = string.Empty;

    [ObservableProperty]
    private bool _isTitleFetched = false;

    [ObservableProperty]
    private string _titleFetchStatus = string.Empty;

    [ObservableProperty]
    private bool _isWaitingForProceed = false;

    [ObservableProperty]
    private string _addToQueueButtonText = "Add to Queue";

    [ObservableProperty]
    private bool _isProceedButtonState = false;

    [ObservableProperty]
    private ObservableCollection<DownloadQueueItem> _queueItems = [];

    private bool _isQueueProcessing = false;

    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Command to add URL to download queue
    /// Two-step process when EditTitle is checked:
    /// 1. First click: Fetch title and wait for user to edit
    /// 2. Second click (Proceed): Add to queue with custom title
    /// Includes duplicate detection: prompts user if video was already downloaded
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteAddToQueue))]
    private async Task AddToQueue()
    {
        if (!ValidateUrl())
        {
            UpdateStatus("Invalid YouTube URL");
            return;
        }

        // If EditTitle is checked and we haven't fetched the title yet
        if (EditTitle && !IsWaitingForProceed)
        {
            // Step 1: Fetch title and wait for user to edit
            await FetchVideoTitleAsync();

            if (IsTitleFetched)
            {
                // Change button to "Proceed" state
                IsWaitingForProceed = true;
                AddToQueueButtonText = "Proceed";
                IsProceedButtonState = true;
            }
            return;
        }

        // Check for duplicates before adding to queue
        try
        {
            var existingRecord = await checkDuplicateUseCase.GetExistingRecordAsync(YoutubeUrl);
            if (existingRecord is not null)
            {
                // Show duplicate warning (for now, just log - we'll add UI dialog later)
                var message = $"This video was already downloaded on {existingRecord.DownloadedAt}:\n" +
                              $"Title: {existingRecord.VideoTitle}\n\n" +
                              $"Do you want to download it again?";

                Console.WriteLine($"[DUPLICATE DETECTED] {message}");

                // TODO: Show user dialog and get confirmation
                // For now, we'll continue with the download
                UpdateStatus($"Warning: Video already downloaded on {existingRecord.DownloadedAt}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to check duplicates: {ex.Message}");
            // Continue with download even if duplicate check fails
        }

        // Step 2: Proceed with adding to queue (either EditTitle is unchecked or user clicked Proceed)
        var queueItem = new DownloadQueueItem
        {
            Url = YoutubeUrl,
            Status = DownloadStatus.Pending,
            StatusMessage = "Pending",
            CustomTitle = EditTitle && IsTitleFetched ? CustomTitle : null
        };

        QueueItems.Add(queueItem);
        ClearUrl();
        ClearTitleEdit();
        ResetButtonState();
        UpdateStatus($"Added to queue. {QueueItems.Count(i => i.Status == DownloadStatus.Pending)} items pending.");

        // Start processing queue if not already running
        if (!_isQueueProcessing)
        {
            _ = ProcessQueueAsync();
        }
    }

    private async Task ProcessQueueAsync()
    {
        _isQueueProcessing = true;

        while (QueueItems.Any(i => i.Status == DownloadStatus.Pending))
        {
            var nextItem = QueueItems.First(i => i.Status == DownloadStatus.Pending);

            // Update item status to Downloading
            nextItem.Status = DownloadStatus.Downloading;
            nextItem.StatusMessage = "Starting download...";

            // Create new cancellation token source for this download
            _cancellationTokenSource = new CancellationTokenSource();
            SetProcessingState(true);

            try
            {
                // Use the DownloadAndSendUseCase to orchestrate the entire workflow
                var progress = new Progress<double>(value => nextItem.Progress = value);

                var downloadResult = await downloadAndSendUseCase.ExecuteAsync(
                    nextItem.Url,
                    nextItem.CustomTitle,
                    progress,
                    _cancellationTokenSource.Token);

                // Handle result using pattern matching
                if (!downloadResult.IsSuccess)
                {
                    var error = downloadResult.Error!;
                    nextItem.Status = DownloadStatus.Failed;
                    nextItem.StatusMessage = "Failed";
                    nextItem.ErrorMessage = GetUserFriendlyErrorMessage(error);
                    nextItem.Progress = 0;

                    Console.WriteLine($"Download failed: {error.Code} - {error.Message}");
                }
                else
                {
                    // Success!
                    nextItem.DownloadResult = downloadResult.Value!;
                    nextItem.Status = DownloadStatus.Completed;
                    nextItem.StatusMessage = "✓ Completed";
                    nextItem.Progress = 100;
                }
            }
            catch (OperationCanceledException)
            {
                nextItem.Status = DownloadStatus.Cancelled;
                nextItem.StatusMessage = "Cancelled";
                nextItem.Progress = 0;
            }
            finally
            {
                // Cleanup temp files after processing this item
                if (nextItem.DownloadResult != null)
                {
                    // Cleanup media file
                    try
                    {
                        if (File.Exists(nextItem.DownloadResult.TempMediaFilePath))
                        {
                            File.Delete(nextItem.DownloadResult.TempMediaFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to cleanup temp media file: {ex.Message}");
                    }

                    // Cleanup thumbnail
                    if (nextItem.DownloadResult.TempThumbnailPath != null)
                    {
                        try
                        {
                            if (File.Exists(nextItem.DownloadResult.TempThumbnailPath))
                            {
                                File.Delete(nextItem.DownloadResult.TempThumbnailPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to cleanup temp thumbnail: {ex.Message}");
                        }
                    }
                }

                // Dispose and clear the cancellation token source
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                SetProcessingState(false);
            }
        }

        _isQueueProcessing = false;
        UpdateStatus($"Queue completed. {QueueItems.Count(i => i.Status == DownloadStatus.Completed)} succeeded, {QueueItems.Count(i => i.Status == DownloadStatus.Failed)} failed.");
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCancel))]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        UpdateStatus("Cancelling download...");
    }

    private bool CanExecuteCancel() => IsProcessing && _cancellationTokenSource != null;

    private bool CanExecuteAddToQueue() => ValidateUrl();

    private bool ValidateUrl()
    {
        if (string.IsNullOrWhiteSpace(YoutubeUrl))
            return false;

        return YouTubeUrl.TryCreate(YoutubeUrl, out _);
    }

    private void SetProcessingState(bool isProcessing)
    {
        IsProcessing = isProcessing;
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void UpdateStatus(string message) => StatusMessage = message;

    private void ClearUrl() => YoutubeUrl = string.Empty;

    private void ClearTitleEdit()
    {
        EditTitle = false;
        CustomTitle = string.Empty;
        IsTitleFetched = false;
        TitleFetchStatus = string.Empty;
    }

    private void ResetButtonState()
    {
        IsWaitingForProceed = false;
        AddToQueueButtonText = "Add to Queue";
        IsProceedButtonState = false;
    }

    partial void OnYoutubeUrlChanged(string value)
    {
        AddToQueueCommand.NotifyCanExecuteChanged();

        IsTitleFetched = false;
        CustomTitle = string.Empty;
        TitleFetchStatus = string.Empty;

        ResetButtonState();
    }

    partial void OnEditTitleChanged(bool value)
    {
        if (!value)
        {
            // Reset button state when unchecking EditTitle
            ResetButtonState();
        }
    }

    private async Task FetchVideoTitleAsync()
    {
        TitleFetchStatus = "Fetching video title...";
        IsTitleFetched = false;

        var titleResult = await getVideoInfoUseCase.GetVideoTitleAsync(YoutubeUrl);

        if (!titleResult.IsSuccess)
        {
            var error = titleResult.Error!;
            TitleFetchStatus = $"Failed to fetch title: {GetUserFriendlyErrorMessage(error)}";
            IsTitleFetched = false;
            Console.WriteLine($"Title fetch failed: {error.Code} - {error.Message}");
        }
        else
        {
            CustomTitle = titleResult.Value!;
            IsTitleFetched = true;
            TitleFetchStatus = "Edit the title above if needed";
        }
    }

    /// <summary>
    /// Maps error codes to user-friendly error messages.
    /// </summary>
    private static string GetUserFriendlyErrorMessage(Error error)
    {
        return error.Code switch
        {
            "YouTube.InvalidUrl" => "Invalid YouTube URL. Please check the link and try again.",
            "YouTube.VideoIdExtractionFailed" => "Unable to process YouTube URL. Please verify the link is correct.",
            "YouTube.DownloadFailed" => "Failed to download video. Please check your internet connection and try again.",
            "YouTube.TitleFetchFailed" => "Unable to fetch video information. Please check the URL and try again.",
            "YouTube.FileNotFound" => "Downloaded file not found. The download may have failed.",
            "YouTube.ProcessStartFailed" => "Failed to start download process. Please ensure yt-dlp is installed.",
            "Telegram.BotTokenNotConfigured" => "Telegram bot not configured. Please set bot token in Settings.",
            "Telegram.ChatIdNotConfigured" => "Telegram chat not configured. Please set chat ID in Settings.",
            "Telegram.SendFailed" => "Failed to send to Telegram. Please verify your bot configuration in Settings.",
            "Telegram.InitializationFailed" => "Failed to initialize Telegram bot. Please check your bot token in Settings.",
            _ => $"An error occurred: {error.Message}"
        };
    }
}
