using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Infrastructure.Queue;

public sealed class DownloadQueueProcessor(
    IDownloadQueue queue,
    Func<QueueItem, IProgress<double>, CancellationToken, Task<Result>> processItemCallback) : IDownloadQueueProcessor
{
    private readonly Queue<Guid> _pendingItems = [];
    private readonly Dictionary<Guid, CancellationTokenSource> _cancellationTokens = [];

    private bool _isProcessing;

    public void Enqueue(Guid queueItemId)
    {
        _pendingItems.Enqueue(queueItemId);

        if (_isProcessing) return;
        _isProcessing = true;

        _ = ProcessLoopAsync();
    }

    public void Proceed(Guid itemId)
    {
        var queueItem = queue.GetItem(itemId);
        if (!queueItem.CanStartDownload) return;

        queueItem.Status = DownloadStatus.Pending;
        queue.UpdateItem(queueItem.Id);

        Enqueue(itemId);
    }

    public void Cancel(Guid itemId)
    {
        // Cancel `Downloading` item
        if (_cancellationTokens.TryGetValue(itemId, out var cancellationToken))
        {
            cancellationToken.Cancel();
            return;
        }

        // Mark `Pending` item as `Cancelled` to not process it in queue
        var queueItem = queue.GetItem(itemId);
        if (!queueItem.CanCancel) return;

        queueItem.Status = DownloadStatus.Cancelled;
        queue.UpdateItem(queueItem.Id);
    }

    private async Task ProcessLoopAsync()
    {
        try
        {
            while (true)
            {
                if (!_pendingItems.TryDequeue(out var itemId))
                {
                    _isProcessing = false;
                    return;
                }

                var item = queue.GetItem(itemId);
                await ProcessItemAsync(item);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task ProcessItemAsync(QueueItem queueItem)
    {
        if (queueItem.Status != DownloadStatus.Pending)
            return;

        queueItem.Status = DownloadStatus.Downloading;
        queueItem.Error = null;
        queueItem.Progress = 0;

        queue.UpdateItem(queueItem.Id);

        _cancellationTokens[queueItem.Id] = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(value =>
            {
                queueItem.Progress = value;
                queue.UpdateItem(queueItem.Id);
            });

            var result = await processItemCallback(
                queueItem,
                progress,
                _cancellationTokens[queueItem.Id].Token);

            if (result.IsSuccess)
            {
                queueItem.Status = DownloadStatus.Completed;
            }
            else
            {
                queueItem.Status = DownloadStatus.Failed;
                queueItem.Error = result.Error;
            }
        }
        catch (OperationCanceledException)
        {
            queueItem.Status = DownloadStatus.Cancelled;
            queueItem.Progress = 0d;
        }
        catch (Exception ex)
        {
            queueItem.Status = DownloadStatus.Failed;
            queueItem.Error = new Error(ex.Message);
        }
        finally
        {
            _cancellationTokens[queueItem.Id].Dispose();
            _cancellationTokens.Remove(queueItem.Id);

            queue.UpdateItem(queueItem.Id);
        }
    }
}
