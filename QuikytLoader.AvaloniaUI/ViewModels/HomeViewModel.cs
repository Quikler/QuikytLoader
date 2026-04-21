using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.Models;
using System.Threading.Tasks;

namespace QuikytLoader.AvaloniaUI.ViewModels;

/// <summary>
/// ViewModel for the Home page (YouTube download functionality)
/// </summary>
public partial class HomeViewModel(
    ValidateYouTubeUrlUseCase validateYouTubeUrlUseCase,
    QueueAdditionService queueAdditionService,
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

        UpdateStatus("Adding to queue...");
        var result = await queueAdditionService.AddAsync(YoutubeUrl, UseCustomTitle, ConfirmDuplicateAsync);

        UpdateStatus(FormatResult(result));

        if (result is QueueAdditionResult.SingleAdded or QueueAdditionResult.PlaylistAdded)
            YoutubeUrl = string.Empty;
    }

    [RelayCommand]
    private void ProceedItem(DownloadQueueItem item) => queueManager.Proceed(item);

    [RelayCommand]
    private void ProceedGroup(string groupId) => queueManager.ProceedGroup(groupId);

    private async Task<bool> ConfirmDuplicateAsync(DownloadHistoryDto existing)
    {
        var message = $"This video was already downloaded on {existing.DownloadedAt}:\n" +
                      $"Title: {existing.VideoTitle}\n\n" +
                      $"Do you want to download it again?";
        return await dialogService.ShowConfirmationAsync("Duplicate Detected", message);
    }

    private string FormatResult(QueueAdditionResult result) => result switch
    {
        QueueAdditionResult.SingleAdded r => $"Added to queue. {r.QueueCount} items in queue.",
        QueueAdditionResult.PlaylistAdded r => $"Added playlist '{r.PlaylistTitle}' ({r.VideoCount} videos).",
        QueueAdditionResult.AlreadyQueued => "Playlist already in queue.",
        QueueAdditionResult.DuplicateCancelled => "Download cancelled - video already exists.",
        QueueAdditionResult.Failed f => $"Error: {f.Message}",
        _ => string.Empty
    };

    private bool CanExecuteAddToQueue() => validateYouTubeUrlUseCase.IsValid(YoutubeUrl);

    private void UpdateStatus(string message) => StatusMessage = message;

    partial void OnYoutubeUrlChanged(string value)
    {
        AddToQueueCommand.NotifyCanExecuteChanged();
    }
}
