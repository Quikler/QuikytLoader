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
    /// All queue entries (single-video groups and playlist groups). Single source of truth for the UI.
    /// </summary>
    public ObservableCollection<QueueGroupViewModel> Queue { get; } = [];

    /// <summary>
    /// Flattened enumeration of every queue item (across all groups). Used for processing and dedup.
    /// </summary>
    public IEnumerable<DownloadQueueItem> AllItems =>
        Queue.SelectMany(g => g.Items);

    [ObservableProperty]
    private bool _isProcessing;

    private readonly Queue<DownloadQueueItem> _itemsToDownload = [];

    private CancellationTokenSource? _currentCancellationTokenSource;

    /// <summary>
    /// Enqueue a standalone single-video item (wraps it in a single-item, non-playlist group).
    /// </summary>
    public void Enqueue(DownloadQueueItem item)
    {
        var groupId = $"single:{Guid.NewGuid():N}";
        var group = new QueueGroupViewModel(groupId, string.Empty, isPlaylist: false, ProceedGroup);
        item.GroupId = groupId;
        group.Items.Add(item);
        Queue.Add(group);

        if (item.Status == DownloadStatus.Pending)
            _ = ProcessItemsToDownloadAsync(item);
    }

    /// <summary>
    /// Enqueue a batch of items belonging to a playlist group. Items keep their existing Status
    /// (disabled items remain disabled and will be skipped by processing).
    /// </summary>
    public void EnqueueGroup(string groupId, string playlistTitle, IEnumerable<DownloadQueueItem> items)
    {
        var group = new QueueGroupViewModel(groupId, playlistTitle, isPlaylist: true, ProceedGroup);
        foreach (var item in items)
        {
            item.GroupId = groupId;
            item.IsInPlaylist = true;
            group.Items.Add(item);
        }
        Queue.Add(group);
        group.RecomputeCounts();
    }

    public bool HasGroup(string groupId) => Queue.Any(g => g.GroupId == groupId);

    /// <summary>
    /// Look up an item by YouTube id across all groups. Used to detect duplicates
    /// when enqueuing a new playlist.
    /// </summary>
    public DownloadQueueItem? TryFindByYoutubeId(string youtubeId, string? excludeGroupId = null)
    {
        if (string.IsNullOrWhiteSpace(youtubeId)) return null;
        return AllItems.FirstOrDefault(i =>
            i.YoutubeId == youtubeId &&
            (excludeGroupId is null || i.GroupId != excludeGroupId));
    }

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
        var group = Queue.FirstOrDefault(g => g.GroupId == groupId);
        if (group is null) return;

        foreach (var item in group.Items)
        {
            if (item.Status == DownloadStatus.Disabled || !item.IsSelected || (item.Status is DownloadStatus.Downloading or DownloadStatus.Completed)) continue;
            item.Status = DownloadStatus.Pending;

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
        _itemsToDownload.Enqueue(itemToQueue);

        if (IsProcessing)
            return;

        IsProcessing = true;
        try
        {
            DownloadQueueItem? currentItem;
            while ((currentItem = _itemsToDownload.Dequeue()) is not null && currentItem.Status == DownloadStatus.Pending)
            {
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
            IsProcessing = false;
        }
    }
}
