using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.UseCases;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Infrastructure.Queue;

public sealed class DownloadQueueProcessor(
    IDownloadQueue queue,
    DownloadAndSendUseCase downloadAndSendUseCase) : IDownloadQueueProcessor
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
        if (queueItem is null || !queueItem.CanStartDownload) return;

        switch (queueItem.Status)
        {
            case DownloadStatus.Editing:
            case DownloadStatus.Queued:
            case DownloadStatus.Failed:
            case DownloadStatus.Cancelled:
                queueItem.Status = DownloadStatus.Pending;
                break;

            default:
                Console.WriteLine($"Item '{queueItem.Metadata?.Title}' is not in correct State for ProceedAsync: {queueItem.Status}");
                return;
        }

        queue.UpdateItem(queueItem.Id);
        Enqueue(itemId);
    }

    public void Cancel(Guid itemId) => _cancellationTokens[itemId].Cancel();

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
                if (item is null) continue;

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

        queue.UpdateItem(queueItem.Id);

        _cancellationTokens[queueItem.Id] = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(value =>
            {
                queueItem.Progress = value;
                queue.UpdateItem(queueItem.Id);
            });

            var result = await downloadAndSendUseCase.ExecuteAsync(
                queueItem.Source,
                queueItem.CustomTitle,
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
