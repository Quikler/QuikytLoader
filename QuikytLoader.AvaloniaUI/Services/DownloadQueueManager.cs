using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.Services;

public partial class DownloadQueueManager(
    Func<DownloadQueueItem, CancellationToken, Task<Result>> processQueueItem) : ObservableObject
{
    public ObservableCollection<DownloadQueueItem> Items { get; } = [];

    [ObservableProperty]
    private bool _isProcessing;

    private CancellationTokenSource? _currentCancellationTokenSource;

    public void Enqueue(DownloadQueueItem item)
    {
        Items.Add(item);

        if (item.Status == DownloadStatus.Pending)
            _ = ProcessQueueAsync();
    }

    public void Proceed(DownloadQueueItem item)
    {
        if (item.Status != DownloadStatus.Editing) return;

        item.Status = DownloadStatus.Pending;
        _ = ProcessQueueAsync();
    }

    // TODO: Wire to per-item cancel button (see TO-DOS.md)
    public void CancelCurrent() => _currentCancellationTokenSource?.Cancel();

    public string GetStatusSummary()
    {
        var editingCount = Items.Count(i => i.Status == DownloadStatus.Editing);
        var succeededCount = Items.Count(i => i.Status == DownloadStatus.Completed);
        var failedCount = Items.Count(i => i.Status == DownloadStatus.Failed);

        return $"Queue processed. {succeededCount} succeeded, {failedCount} failed.{(editingCount > 0 ? $" {editingCount} items waiting for edits." : string.Empty)}";
    }

    private async Task ProcessQueueAsync()
    {
        if (IsProcessing)
            return;

        IsProcessing = true;
        try
        {
            DownloadQueueItem? currentItem;
            while ((currentItem = Items.FirstOrDefault(i => i.Status == DownloadStatus.Pending)) is not null)
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
