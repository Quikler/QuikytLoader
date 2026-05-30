using System.Collections.ObjectModel;
using QuikytLoader.AvaloniaUI.Models;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public interface IQueueItemsViewModel
{
    public string Id { get; }

    public ObservableCollection<DownloadQueueItem> Items { get; }
}
