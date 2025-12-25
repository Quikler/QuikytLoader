using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.Domain.Enums;
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
    FindExistingDownloadUseCase findExistingDownloadUseCase,
    GetVideoTitleUseCase getVideoTitleUseCase,
    ValidateYouTubeUrlUseCase validateYouTubeUrlUseCase) : ViewModelBase
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
    private bool _useCustomTitle = false;

    [ObservableProperty]
    private string _customTitle = string.Empty;

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
    /// Command to add URL to download queue.
    /// Two-step process when UseCustomTitle is checked:
    /// 1. First click: Fetch title and wait for user to edit
    /// 2. Second click (Proceed): Add to queue with custom title
    /// Includes duplicate detection: prompts user if video was already downloaded.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteAddToQueue))]
    private async Task AddToQueue()
    {
        if (!validateYouTubeUrlUseCase.IsValid(YoutubeUrl))
        {
            UpdateStatus("Invalid YouTube URL");
            return;
        }

        // If UseCustomTitle is checked and we haven't fetched the title yet
        if (UseCustomTitle && !IsWaitingForProceed)
        {
            // Step 1: Fetch title and wait for user to edit
            TitleFetchStatus = "Fetching video title...";

            var titleResult = await getVideoTitleUseCase.GetVideoTitleAsync(YoutubeUrl);
            if (!titleResult.IsSuccess)
            {
                TitleFetchStatus = $"Failed to fetch title: {titleResult.Error.Message}";
                Console.WriteLine($"Title fetch failed: {titleResult.Error.Message}");
                return;
            }

            CustomTitle = titleResult.Value;
            TitleFetchStatus = "Edit the title above if needed";

            // Change button to "Proceed" state
            IsWaitingForProceed = true;
            AddToQueueButtonText = "Proceed";
            IsProceedButtonState = true;
            return;
        }

        var duplicateCheckResult = await findExistingDownloadUseCase.FindAsync(YoutubeUrl);
        if (!duplicateCheckResult.IsSuccess)
        {
            UpdateStatus($"Error: {duplicateCheckResult.Error.Message}");
            Console.WriteLine($"Duplicate check failed: {duplicateCheckResult.Error.Message}");
            return;
        }

        if (duplicateCheckResult.Value is not null)
        {
            var existingRecord = duplicateCheckResult.Value;
            var message = $"This video was already downloaded on {existingRecord.DownloadedAt}:\n" +
                          $"Title: {existingRecord.VideoTitle}\n\n" +
                          $"Do you want to download it again?";

            Console.WriteLine($"[DUPLICATE DETECTED] {message}");

            // TODO: Show user dialog and get confirmation
            // For now, we'll continue with the download
            UpdateStatus($"Warning: Video already downloaded on {existingRecord.DownloadedAt}");
        }
        // If duplicateCheckResult.Value is null, no duplicate exists - continue silently

        // Step 2: Proceed with adding to queue
        var queueItem = new DownloadQueueItem
        {
            Url = YoutubeUrl,
            Status = DownloadStatus.Pending,
            StatusMessage = "Pending",
            CustomTitle = UseCustomTitle ? CustomTitle : null
        };

        QueueItems.Add(queueItem);
        ClearUrl();
        ClearTitleEdit();
        ResetButtonState();
        UpdateStatus($"Added to queue. {QueueItems.Count(i => i.Status == DownloadStatus.Pending)} items pending.");

        if (!_isQueueProcessing)
            _ = ProcessQueueAsync();
    }

    private async Task ProcessQueueAsync()
    {
        _isQueueProcessing = true;
        while (QueueItems.Any(i => i.Status == DownloadStatus.Pending))
        {
            var nextItem = QueueItems.First(i => i.Status == DownloadStatus.Pending);
            nextItem.Status = DownloadStatus.Downloading;
            nextItem.StatusMessage = "Starting download...";

            _cancellationTokenSource = new CancellationTokenSource();
            SetProcessingState(true);

            try
            {
                var progress = new Progress<double>(value => nextItem.Progress = value);
                var downloadResult = await downloadAndSendUseCase.ExecuteAsync(
                    nextItem.Url,
                    nextItem.CustomTitle,
                    progress,
                    _cancellationTokenSource.Token);

                if (!downloadResult.IsSuccess)
                {
                    var error = downloadResult.Error;
                    nextItem.Status = DownloadStatus.Failed;
                    nextItem.StatusMessage = "Failed";
                    nextItem.ErrorMessage = error.Message;
                    nextItem.Progress = 0;

                    Console.WriteLine($"Download failed: {error.Message}");
                }
                else
                {
                    nextItem.DownloadResult = downloadResult.Value;
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
                if (nextItem.DownloadResult is not null)
                {
                    File.Delete(nextItem.DownloadResult.TempMediaFilePath);
                    File.Delete(nextItem.DownloadResult.TempThumbnailPath);
                }

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

    private bool CanExecuteAddToQueue() => validateYouTubeUrlUseCase.IsValid(YoutubeUrl);

    private void SetProcessingState(bool isProcessing)
    {
        IsProcessing = isProcessing;
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void UpdateStatus(string message) => StatusMessage = message;

    private void ClearUrl() => YoutubeUrl = string.Empty;

    private void ClearTitleEdit()
    {
        UseCustomTitle = false;
        CustomTitle = string.Empty;
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

        CustomTitle = string.Empty;
        TitleFetchStatus = string.Empty;
        ResetButtonState();
    }

    partial void OnUseCustomTitleChanged(bool value)
    {
        if (!value) ResetButtonState();
    }
}
