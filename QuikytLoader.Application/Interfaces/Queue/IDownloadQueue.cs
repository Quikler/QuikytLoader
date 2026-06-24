using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Queue;

public abstract record QueueEvent
{
    public sealed record ItemAdded(QueueItem Item) : QueueEvent;
    public sealed record ItemUpdated(Guid ItemId) : QueueEvent;
    public sealed record GroupAdded(QueueGroup Group) : QueueEvent;
    public sealed record QueueCleared() : QueueEvent;
}

public interface IDownloadQueue
{
    int ItemsCount { get; }

    void EnqueueItem(QueueItem item);
    void EnqueueGroup(QueueGroup group, IEnumerable<QueueItem> items);

    QueueItem GetItem(Guid id);
    void UpdateItem(Guid itemId);

    bool ContainsGroup(string groupId);
    bool ContainsSourceId(string sourceId);

    event Action<QueueEvent> Changed;
}
