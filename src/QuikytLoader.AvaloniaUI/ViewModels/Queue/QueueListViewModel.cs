using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue;

public partial class QueueListViewModel(DownloadQueueManager queueManager) : ViewModelBase
{
    [ObservableProperty]
    private QueueGroupViewModel? _stickyQueueGroup;

    public DownloadQueueManager QueueManager => queueManager;
}
