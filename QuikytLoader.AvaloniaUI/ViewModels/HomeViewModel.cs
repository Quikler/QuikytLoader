using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace QuikytLoader.AvaloniaUI.ViewModels;

/// <summary>
/// ViewModel for the Home page (YouTube download functionality)
/// </summary>
public partial class HomeViewModel(
    FindExistingDownloadUseCase findExistingDownloadUseCase,
    GetVideoMetadataUseCase getVideoMetadataUseCase,
    ValidateYouTubeUrlUseCase validateYouTubeUrlUseCase,
    DownloadQueueManager queueManager,
    IDialogService dialogService) : ViewModelBase
{
    [ObservableProperty]
    private string _youtubeUrl = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _useCustomTitle = false;

    public DownloadQueueManager QueueManager => queueManager;

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

        var item = new DownloadQueueItem
        {
            Url = YoutubeUrl,
            Status = UseCustomTitle ? DownloadStatus.Editing : DownloadStatus.Pending
        };

        queueManager.Enqueue(item);
        _ = FetchMetadataAsync(item);

        YoutubeUrl = string.Empty;
        UpdateStatus($"Added to queue. {queueManager.Items.Count} items in queue.");
    }

    [RelayCommand]
    private void ProceedItem(DownloadQueueItem item) => queueManager.Proceed(item);

    [RelayCommand(CanExecute = nameof(CanExecuteCancel))]
    private void Cancel()
    {
        queueManager.CancelCurrent();
        UpdateStatus("Cancelling download...");
    }

    private async Task FetchMetadataAsync(DownloadQueueItem item)
    {
        var videoMetadata = await getVideoMetadataUseCase.GetMetadataAsync(item.Url);

        if (videoMetadata.IsSuccess)
            item.ApplyMetadata(videoMetadata.Value);
        else
            item.HasMetadataError = true;
    }

    private bool CanExecuteCancel() => queueManager.IsProcessing;

    private bool CanExecuteAddToQueue() => validateYouTubeUrlUseCase.IsValid(YoutubeUrl);

    private void UpdateStatus(string message) => StatusMessage = message;

    partial void OnYoutubeUrlChanged(string value)
    {
        AddToQueueCommand.NotifyCanExecuteChanged();
    }
}
