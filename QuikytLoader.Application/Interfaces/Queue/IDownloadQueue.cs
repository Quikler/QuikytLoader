using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Queue;

public abstract record QueueEvent
{
    public sealed record ItemAdded(QueueItem Item) : QueueEvent;
    public sealed record ItemUpdated(QueueItem Item) : QueueEvent;
    public sealed record GroupAdded(QueueGroup Group) : QueueEvent;
    public sealed record QueueCleared() : QueueEvent;
}

public interface IDownloadQueue
{
    int ItemsCount { get; }

    void EnqueueItem(QueueItem item);
    void EnqueueGroup(QueueGroup group, IEnumerable<QueueItem> items);

    QueueItem? GetItem(Guid id);
    void UpdateItem(QueueItem item);

    bool ContainsGroup(string groupId);
    bool ContainsItem(Guid itemId);
    bool ContainsSourceId(string externalId);

    event Action<QueueEvent> Changed;
}
