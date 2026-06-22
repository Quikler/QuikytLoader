namespace QuikytLoader.Application.Interfaces.Queue;

public interface IDownloadQueueProcessor
{
    /// <summary>
    /// Adds an item to the processing pipeline.
    /// Processing is sequential and side-effecting:
    /// - Status will be updated to Downloading during execution
    /// - Completion/failure is applied asynchronously
    /// - Only one item is processed at a time
    /// </summary>
    void Enqueue(Guid queueItemId);

    /// <summary>
    /// Transitions item into runnable state and enqueues it.
    /// </summary>
    void Proceed(Guid queueItemId);

    /// <summary>
    /// Cancels downloading queue item by Id.
    /// </summary>
    void Cancel(Guid queueItemId);
}
