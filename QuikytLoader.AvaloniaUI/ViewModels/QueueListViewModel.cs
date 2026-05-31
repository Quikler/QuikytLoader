using CommunityToolkit.Mvvm.Input;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.AvaloniaUI.Services;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class QueueListViewModel(DownloadQueueManager queueManager) : ViewModelBase
{
    public DownloadQueueManager QueueManager => queueManager;

    [RelayCommand]
    private void ProceedItem(DownloadQueueItem item) => queueManager.Proceed(item);
}
