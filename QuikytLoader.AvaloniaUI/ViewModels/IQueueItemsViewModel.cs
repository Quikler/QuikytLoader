using System.Collections.Generic;
using QuikytLoader.AvaloniaUI.Models;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public interface IQueueItemsViewModel
{
    public string Id { get; }

    public IReadOnlyList<DownloadQueueItem> Items { get; }
}
