using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.AvaloniaUI.Services;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class QueueListViewModel(DownloadQueueManager queueManager) : ViewModelBase
{
    [ObservableProperty]
    private QueueGroupViewModel? _stickyQueueGroup;

    public DownloadQueueManager QueueManager => queueManager;
}
