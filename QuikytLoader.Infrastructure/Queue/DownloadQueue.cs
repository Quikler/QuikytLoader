using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Infrastructure.Queue;

public sealed class DownloadQueue : IDownloadQueue
{
    private readonly List<QueueItem> _items = [];
    private readonly Dictionary<Guid, QueueItem> _itemsById = [];
    private readonly Dictionary<string, QueueItem> _itemsBySourceId = [];

    private readonly List<QueueGroup> _groups = [];

    public int ItemsCount => _items.Count;

    public void EnqueueItem(QueueItem item)
    {
        _items.Add(item);
        _itemsById[item.Id] = item;
        _itemsBySourceId[item.Source.SourceId] = item;

        Changed?.Invoke(new QueueEvent.ItemAdded(item));
    }

    public void EnqueueGroup(QueueGroup group, IEnumerable<QueueItem> items)
    {
        _groups.Add(group);

        foreach (var item in items)
        {
            _items.Add(item);
            _itemsById[item.Id] = item;
            _itemsBySourceId[item.Source.SourceId] = item;
        }

        Changed?.Invoke(new QueueEvent.GroupAdded(group));
    }

    public QueueItem? GetItem(Guid id) => _itemsById.GetValueOrDefault(id);

    public void UpdateItem(QueueItem item)
    {
        _itemsById[item.Id] = item;
        _itemsBySourceId[item.Source.SourceId] = item;

        Changed?.Invoke(new QueueEvent.ItemUpdated(item));
    }

    public bool ContainsGroup(string groupId) => _groups.Any(g => g.Id == groupId);

    public bool ContainsItem(Guid itemId) => _itemsById.ContainsKey(itemId);

    public bool ContainsSourceId(string externalId) => _itemsBySourceId.ContainsKey(externalId);

    public event Action<QueueEvent>? Changed;
}
