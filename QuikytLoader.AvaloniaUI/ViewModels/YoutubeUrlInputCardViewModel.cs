using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class YoutubeUrlInputCardViewModel(
    ValidateYouTubeUrlUseCase validateYouTubeUrlUseCase,
    QueueAdditionService queueAdditionService,
    IDialogService dialogService,
    IUiNotificationService uiNotificationService) : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddToQueueCommand))]
    private string _youtubeUrl = string.Empty;

    [ObservableProperty]
    private bool _useCustomTitle = false;

    [RelayCommand(CanExecute = nameof(CanExecuteAddToQueue))]
    private async Task AddToQueue()
    {
        uiNotificationService.SetMessageInfo("Adding to queue...");
        var result = await queueAdditionService.AddAsync(YoutubeUrl, UseCustomTitle, ConfirmDuplicateAsync);

        uiNotificationService.SetMessageInfo(FormatResult(result));

        if (result is QueueAdditionResult.SingleAdded or QueueAdditionResult.PlaylistAdded)
            YoutubeUrl = string.Empty;
    }

    private bool CanExecuteAddToQueue() => validateYouTubeUrlUseCase.IsValid(YoutubeUrl);

    private async Task<bool> ConfirmDuplicateAsync(DownloadHistoryDto existing)
    {
        return await dialogService.ShowConfirmationAsync("Duplicate Detected",
            $"""
            This video was already downloaded at {existing.DownloadedAt}:
            Title: {existing.VideoTitle}

            Do you want to download it again?
            """);
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
}
