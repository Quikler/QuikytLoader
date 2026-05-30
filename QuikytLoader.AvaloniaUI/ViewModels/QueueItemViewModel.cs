using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.AvaloniaUI.Models;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class QueueItemViewModel(string id, DownloadQueueItem item, Action<string> proceedItemCallback) : ViewModelBase, IQueueItemsViewModel
{
    public string Id { get; } = id;

    public IReadOnlyList<DownloadQueueItem> Items { get; } = [item];

    private readonly Action<string> _proceedItemCallback = proceedItemCallback;

    [RelayCommand]
    private void Proceed() => _proceedItemCallback(Id);
}
