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

    private CancellationTokenSource? _currentCancellationTokenSource;

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
        var item = queue.GetItem(itemId);
        if (item is null || !item.CanStartDownload) return;

        switch (item.Status)
        {
            case DownloadStatus.Editing:
            case DownloadStatus.Queued:
            case DownloadStatus.Failed:
            case DownloadStatus.Cancelled:
                item.Status = DownloadStatus.Pending;
                break;

            default:
                Console.WriteLine($"Item '{item.Metadata?.Title}' is not in correct State for ProceedAsync: {item.Status}");
                return;
        }

        queue.UpdateItem(item);
        Enqueue(itemId);
    }

    public void CancelCurrent() => _currentCancellationTokenSource?.Cancel();

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

    private async Task ProcessItemAsync(QueueItem item)
    {
        if (item.Status != DownloadStatus.Pending)
            return;

        item.Status = DownloadStatus.Downloading;
        item.Error = null;

        queue.UpdateItem(item);

        _currentCancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(value =>
            {
                item.Progress = value;
                queue.UpdateItem(item);
            });

            var result = await downloadAndSendUseCase.ExecuteAsync(
                item.Source.Url,
                item.CustomTitle,
                progress,
                _currentCancellationTokenSource.Token);

            if (result.IsSuccess)
            {
                item.Status = DownloadStatus.Completed;
            }
            else
            {
                item.Status = DownloadStatus.Failed;
                item.Error = result.Error;
            }
        }
        catch (OperationCanceledException)
        {
            item.Status = DownloadStatus.Cancelled;
        }
        catch (Exception ex)
        {
            item.Status = DownloadStatus.Failed;
            item.Error = new Error(ex.Message);
        }
        finally
        {
            _currentCancellationTokenSource.Dispose();
            _currentCancellationTokenSource = null;

            queue.UpdateItem(item);
        }
    }
}
