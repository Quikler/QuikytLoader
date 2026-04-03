using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;
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
/// </summary>
public partial class HomeViewModel(
    DownloadAndSendUseCase downloadAndSendUseCase,
    FindExistingDownloadUseCase findExistingDownloadUseCase,
    GetVideoMetadataUseCase getVideoMetadataUseCase,
    ValidateYouTubeUrlUseCase validateYouTubeUrlUseCase,
    IDialogService dialogService) : ViewModelBase
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
    private ObservableCollection<DownloadQueueItem> _queueItems = [];

    private bool _isQueueProcessing = false;

    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Command to add URL to download queue.
    /// Adds item immediately, fetches metadata in parallel.
    /// If UseCustomTitle is checked, item gets WaitingForEdits status.
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

            var confirmed = await dialogService.ShowConfirmationAsync("Duplicate Detected", message);
            if (!confirmed)
            {
                UpdateStatus("Download cancelled - video already exists");
                return;
            }
        }

        var queueItem = new DownloadQueueItem
        {
            Url = YoutubeUrl,
            Status = UseCustomTitle ? DownloadStatus.Editing : DownloadStatus.Pending,
            StatusMessage = UseCustomTitle ? "Waiting for title edit" : "Pending"
        };

        if (UseCustomTitle)
        {
            queueItem.ProceedCommand = new RelayCommand(() =>
            {
                queueItem.Status = DownloadStatus.Pending;
                queueItem.StatusMessage = "Pending";

                if (!_isQueueProcessing)
                    _ = ProcessQueueAsync();
            });
        }

        QueueItems.Add(queueItem);
        ClearUrl();
        UpdateStatus($"Added to queue. {QueueItems.Count(i => i.Status == DownloadStatus.Pending)} items pending.");

        // Fetch metadata in parallel (non-blocking)
        _ = FetchMetadataAsync(queueItem);

        if (!UseCustomTitle && !_isQueueProcessing)
            _ = ProcessQueueAsync();
    }

    private async Task FetchMetadataAsync(DownloadQueueItem item)
    {
        var result = await getVideoMetadataUseCase.GetMetadataAsync(item.Url);

        if (result.IsSuccess)
        {
            item.VideoTitle = result.Value.Title;
            item.ChannelName = result.Value.Channel;
            item.Duration = result.Value.Duration;
            item.ThumbnailUrl = result.Value.ThumbnailUrl;
            item.IsMetadataLoaded = true;

            // Populate editable title for custom title items
            if (item.Status == DownloadStatus.Editing)
                item.CustomTitle = result.Value.Title;
        }
        else
        {
            item.HasMetadataError = true;
            Console.WriteLine($"Metadata fetch failed for {item.Url}: {result.Error.Message}");
        }
    }

    private async Task ProcessQueueAsync()
    {
        _isQueueProcessing = true;

        DownloadQueueItem? nextItem;
        while ((nextItem = QueueItems.FirstOrDefault(i => i.Status == DownloadStatus.Pending)) is not null)
        {
            nextItem.Status = DownloadStatus.Downloading;
            nextItem.StatusMessage = "Starting download...";

            _cancellationTokenSource = new CancellationTokenSource();
            SetProcessingState(true);

            try
            {
                var downloadResult = await downloadAndSendUseCase.ExecuteAsync(
                    nextItem.Url,
                    nextItem.CustomTitle,
                    new Progress<double>(value => nextItem.Progress = value),
                    _cancellationTokenSource.Token);

                if (!downloadResult.IsSuccess)
                {
                    var errorMessage = downloadResult.Error.Message;
                    nextItem.Status = DownloadStatus.Failed;
                    nextItem.StatusMessage = "Failed";
                    nextItem.ErrorMessage = errorMessage;
                    nextItem.Progress = 0;

                    Console.WriteLine($"Download failed: {errorMessage}");
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
                // Cleanup temp files - failure is non-critical (OS cleans /tmp anyway)
                // and must not kill the queue loop or prevent remaining items from processing
                if (nextItem.DownloadResult is not null)
                {
                    try { File.Delete(nextItem.DownloadResult.TempMediaFilePath); } catch { }
                    try { File.Delete(nextItem.DownloadResult.TempThumbnailPath); } catch { }
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                SetProcessingState(false);
            }
        }

        var waitingCount = QueueItems.Count(i => i.Status == DownloadStatus.Editing);
        var succeededCount = QueueItems.Count(i => i.Status == DownloadStatus.Completed);
        var failedCount = QueueItems.Count(i => i.Status == DownloadStatus.Failed);

        var statusParts = $"Queue processed. {succeededCount} succeeded, {failedCount} failed.";
        if (waitingCount > 0)
            statusParts += $" {waitingCount} items waiting for edits.";

        UpdateStatus(statusParts);
        _isQueueProcessing = false;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCancel))]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        UpdateStatus("Cancelling download...");
    }

    private bool CanExecuteCancel() => IsProcessing && _cancellationTokenSource is not null;

    private bool CanExecuteAddToQueue() => validateYouTubeUrlUseCase.IsValid(YoutubeUrl);

    private void SetProcessingState(bool isProcessing)
    {
        IsProcessing = isProcessing;
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void UpdateStatus(string message) => StatusMessage = message;

    private void ClearUrl() => YoutubeUrl = string.Empty;

    partial void OnYoutubeUrlChanged(string value)
    {
        AddToQueueCommand.NotifyCanExecuteChanged();
    }
}
