using QuikytLoader.AvaloniaUI.Services;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class QueueListViewModel(DownloadQueueManager queueManager) : ViewModelBase
{
    public DownloadQueueManager QueueManager => queueManager;
}
