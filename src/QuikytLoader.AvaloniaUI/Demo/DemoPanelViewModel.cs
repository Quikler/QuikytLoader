using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

namespace QuikytLoader.AvaloniaUI.Demo;

public partial class DemoPanelViewModel(
    YoutubeUrlInputCardViewModel youtubeUrlInputCardViewModel,
    DownloadQueueManager downloadQueueManager) : ViewModelBase
{
    [RelayCommand]
    private async Task DownloadSingleVideo()
    {
        youtubeUrlInputCardViewModel.YoutubeUrl = CreateVideoUrl();

        await youtubeUrlInputCardViewModel.AddToQueueCommand
            .ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task DownloadSingleVideoWithCustomTitle()
    {
        youtubeUrlInputCardViewModel.YoutubeUrl = CreateVideoUrl();
        youtubeUrlInputCardViewModel.UseCustomTitle = true;

        await youtubeUrlInputCardViewModel.AddToQueueCommand
            .ExecuteAsync(null);

        var queueItemViewModel = downloadQueueManager
            .QueueEntries
            .OfType<QueueItemViewModel>()
            .Last();

        queueItemViewModel.CustomTitle = "Random custom title";

        if (queueItemViewModel.ProceedCommand.CanExecute(null))
            queueItemViewModel.ProceedCommand.Execute(null);

        youtubeUrlInputCardViewModel.UseCustomTitle = false;
    }

    [RelayCommand]
    private async Task DownloadRandomItemsInPlaylist()
    {
        youtubeUrlInputCardViewModel.YoutubeUrl = CreatePlaylistUrl();

        await youtubeUrlInputCardViewModel.AddToQueueCommand
            .ExecuteAsync(null);

        var queueGroupViewModel = downloadQueueManager
            .QueueEntries
            .OfType<QueueGroupViewModel>()
            .Last();

        var queueGroupItems = queueGroupViewModel.Items;

        HashSet<int> indexes = [];
        for (int i = 0; i < System.Random.Shared.Next(1, queueGroupItems.Count); i++)
        {
            var index = System.Random.Shared.Next(queueGroupItems.Count);
            if (indexes.Add(index))
            {
                var item = queueGroupItems[index];
                if (!item.IsSelectable) continue;
                item.IsSelected = true;
            }
        }

        if (queueGroupViewModel.ProceedAllCommand.CanExecute(null))
            queueGroupViewModel.ProceedAllCommand.Execute(null);
    }

    [RelayCommand]
    private async Task DownloadAllItemsInPlaylist()
    {
        youtubeUrlInputCardViewModel.YoutubeUrl = CreatePlaylistUrl();

        await youtubeUrlInputCardViewModel.AddToQueueCommand
            .ExecuteAsync(null);

        var queueGroupViewModel = downloadQueueManager
            .QueueEntries
            .OfType<QueueGroupViewModel>()
            .Last();

        var queueGroupItems = queueGroupViewModel.Items;
        foreach (var item in queueGroupItems)
        {
            if (!item.IsSelectable) continue;
            item.IsSelected = true;
        }

        if (queueGroupViewModel.ProceedAllCommand.CanExecute(null))
            queueGroupViewModel.ProceedAllCommand.Execute(null);
    }

    private static string CreateVideoUrl() => $"https://www.youtube.com/watch?v={RandomVideoId()}";
    private static string CreatePlaylistUrl() => $"https://www.youtube.com/watch?v={RandomVideoId()}&list={RandomPlaylistId()}";

    private static string RandomVideoId() => System.Guid.NewGuid().ToString("N")[..11];
    private static string RandomPlaylistId() => $"PL{System.Guid.NewGuid():N}";
}
