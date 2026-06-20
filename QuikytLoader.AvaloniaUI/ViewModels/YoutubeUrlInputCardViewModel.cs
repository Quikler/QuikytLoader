using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.Validators;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class YoutubeUrlInputCardViewModel(
    YoutubeUrlValidator youtubeUrlValidator,
    AddToQueueUseCase addToQueueUseCase,
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

        var addToQueueResult = await addToQueueUseCase.ExecuteAsync(YoutubeUrl, UseCustomTitle);
        switch (addToQueueResult)
        {
            case AddToQueueResult.SingleAdded:
            case AddToQueueResult.PlaylistAdded:
                YoutubeUrl = string.Empty;
                break;

            case AddToQueueResult.DuplicateDetected duplicateDetected:
                var proceed = await dialogService.ShowConfirmationAsync("Duplicate Detected",
                    $"""
                    This video was already downloaded at {duplicateDetected.ExistingDownload.DownloadedAt}:
                    Title: '{duplicateDetected.ExistingDownload.VideoTitle}'

                    Do you want to download it again?
                    """);

                if (proceed)
                {
                    addToQueueResult = await addToQueueUseCase.ExecuteAsync(
                        YoutubeUrl,
                        UseCustomTitle,
                        ignoreDuplicateCheck: true);
                }
                break;

            case AddToQueueResult.AlreadyQueued alreadyQueued:
                await dialogService.ShowWarningAsync("Video already queued",
                    $"Video '{alreadyQueued.VideoId}' already in queue"
                );
                break;

            case AddToQueueResult.PlaylistAlreadyQueued playlistAlreadyQueued:
                await dialogService.ShowWarningAsync("Playlist already queued",
                    $"Playlist '{playlistAlreadyQueued.PlaylistId}' already in queue");
                break;
        }

        uiNotificationService.SetMessageInfo(FormatResult(addToQueueResult));
    }

    // Note: No need to validate URL in UseCases
    private bool CanExecuteAddToQueue => youtubeUrlValidator.Validate(YoutubeUrl).IsSuccess;

    private string FormatResult(AddToQueueResult result) => result switch
    {
        AddToQueueResult.SingleAdded r => $"Added to queue. {r.QueueCount} items in queue.",
        AddToQueueResult.PlaylistAdded r => $"Added playlist '{r.PlaylistTitle}' ({r.ItemCount} videos).",
        AddToQueueResult.AlreadyQueued r => $"Video '{r.VideoId}' already in queue",
        AddToQueueResult.PlaylistAlreadyQueued r => $"Playlist '{r.PlaylistId}' already in queue",
        AddToQueueResult.DuplicateDetected => "Download cancelled - video already exists",
        AddToQueueResult.Failed f => $"Error: {f.Error.Message}",
        _ => string.Empty
    };
}
