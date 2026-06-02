using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.Services;

public partial class DownloadQueueManager(
    Func<DownloadQueueItem, CancellationToken, Task<Result>> processQueueItem) : ObservableObject
{
    /// <summary>
    /// All queue entries. Can be one item and a group.
    /// </summary>
    public ObservableCollection<IQueueItemsViewModel> Queue { get; } = [];

    private IEnumerable<DownloadQueueItem> AllItems => Queue.SelectMany(g => g.Items);

    private readonly Queue<DownloadQueueItem> _itemsToDownload = [];

    private CancellationTokenSource? _currentCancellationTokenSource;
    private bool _isProcessing;

    /// <summary>
    /// Enqueue a standalone single-video item (wraps it in a single-item, non-playlist group).
    /// </summary>
    public void EnqueueItem(DownloadQueueItem item)
    {
        var queueItemViewModel = new QueueItemViewModel(Guid.NewGuid().ToString(), item, ProceedGroup);
        Queue.Add(queueItemViewModel);

        if (item.Status == DownloadStatus.Pending)
            _ = ProcessItemsToDownloadAsync(item);
    }

    /// <summary>
    /// Enqueue a batch of items belonging to a playlist group. Items keep their existing Status
    /// (disabled items remain disabled and will be skipped by processing).
    /// </summary>
    public void EnqueueGroup(string id, string playlistTitle, IEnumerable<DownloadQueueItem> items)
    {
        var group = new QueueGroupViewModel(id, playlistTitle, [.. items], ProceedGroup);
        Queue.Add(group);
    }

    public bool HasGroup(string groupId) => Queue.Any(g => g.Id == groupId);

    /// <summary>
    /// Look up an item by YouTube id across all groups. Used to detect duplicates
    /// when enqueuing a new playlist.
    /// </summary>
    public DownloadQueueItem? TryFindByYoutubeId(string youtubeId, string? excludeGroupId = null) =>
        AllItems.FirstOrDefault(
            i => i.YoutubeId == youtubeId &&
            (excludeGroupId is null || i.GroupId != excludeGroupId));

    public void Proceed(DownloadQueueItem item)
    {
        if (item.Status != DownloadStatus.Editing) return;

        item.Status = DownloadStatus.Pending;
        _ = ProcessItemsToDownloadAsync(item);
    }

    /// <summary>
    /// Queue all selected, eligible items in a playlist group for processing.
    /// </summary>
    public void ProceedGroup(string groupId)
    {
        var group = Queue.FirstOrDefault(g => g.Id == groupId);
        if (group is null) return;

        foreach (var item in group.Items)
        {
            if (!item.IsSelected ||
                item.Status is DownloadStatus.Pending
                    or DownloadStatus.Disabled
                    or DownloadStatus.Downloading
                    or DownloadStatus.Completed) continue;

            item.SetAsPending();
            Console.WriteLine(item.ToString());

            _ = ProcessItemsToDownloadAsync(item);
        }
    }

    // TODO: Wire to per-item cancel button (see TO-DOS.md)
    public void CancelCurrent() => _currentCancellationTokenSource?.Cancel();

    public string GetStatusSummary()
    {
        var editingCount = AllItems.Count(i => i.Status == DownloadStatus.Editing);
        var succeededCount = AllItems.Count(i => i.Status == DownloadStatus.Completed);
        var failedCount = AllItems.Count(i => i.Status == DownloadStatus.Failed);

        return $"Queue processed. {succeededCount} succeeded, {failedCount} failed.{(editingCount > 0 ? $" {editingCount} items waiting for edits." : string.Empty)}";
    }

    private async Task ProcessItemsToDownloadAsync(DownloadQueueItem itemToQueue)
    {
        // TODO: #17
        if (itemToQueue.Status != DownloadStatus.Pending)
        {
            System.Diagnostics.Debug.Fail($"ProcessItemsToDownloadAsync called with non-Pending item {itemToQueue.Url} (status: {itemToQueue.Status}).");
            return; // silently skip in Release
        }

        _itemsToDownload.Enqueue(itemToQueue);

        if (_isProcessing)
            return;

        _isProcessing = true;
        try
        {
            while (_itemsToDownload.TryDequeue(out var currentItem))
            {
                // TODO: #17
                if (currentItem.Status != DownloadStatus.Pending)
                {
                    // Contract violation: only Pending items should reach this queue.
                    System.Diagnostics.Debug.Fail($"Item {currentItem.Url} dequeued with unexpected status {currentItem.Status}.");
                    continue;
                }

                currentItem.Status = DownloadStatus.Downloading;

                _currentCancellationTokenSource = new CancellationTokenSource();

                try
                {
                    var result = await processQueueItem(
                        currentItem,
                        _currentCancellationTokenSource.Token);

                    if (!result.IsSuccess)
                    {
                        currentItem.Status = DownloadStatus.Failed;
                        currentItem.ErrorMessage = result.Error.Message;
                        currentItem.Progress = 0;
                    }
                    else
                    {
                        currentItem.Status = DownloadStatus.Completed;
                        currentItem.Progress = 100;
                    }
                }
                catch (OperationCanceledException)
                {
                    currentItem.Status = DownloadStatus.Cancelled;
                    currentItem.Progress = 0;
                }
                catch (Exception ex)
                {
                    currentItem.Status = DownloadStatus.Failed;
                    currentItem.ErrorMessage = ex.Message;
                    currentItem.Progress = 0;
                }
                finally
                {
                    _currentCancellationTokenSource?.Dispose();
                    _currentCancellationTokenSource = null;
                }
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
