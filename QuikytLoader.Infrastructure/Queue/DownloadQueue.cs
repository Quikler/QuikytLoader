using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Infrastructure.Queue;

public sealed class DownloadQueue : IDownloadQueue
{
    private readonly Dictionary<Guid, QueueItem> _itemsById = [];
    private readonly Dictionary<string, QueueItem> _itemsBySourceId = [];

    private readonly Dictionary<string, QueueGroup> _groupsById = [];

    public int ItemsCount => _itemsById.Values.Count;

    public void EnqueueItem(QueueItem item)
    {
        _itemsById[item.Id] = item;
        _itemsBySourceId[item.Source.YoutubeVideoId] = item;

        Changed?.Invoke(new QueueEvent.ItemAdded(item));
    }

    public void EnqueueGroup(QueueGroup group, IEnumerable<QueueItem> items)
    {
        _groupsById[group.Id] = group;

        foreach (var item in items)
        {
            _itemsById[item.Id] = item;
            _itemsBySourceId[item.Source.YoutubeVideoId] = item;
        }

        Changed?.Invoke(new QueueEvent.GroupAdded(group));
    }

    public QueueItem? GetItem(Guid id) => _itemsById.GetValueOrDefault(id);
    public void UpdateItem(Guid itemId) => Changed?.Invoke(new QueueEvent.ItemUpdated(itemId));

    public bool ContainsGroup(string groupId) => _groupsById.ContainsKey(groupId);
    public bool ContainsItem(Guid itemId) => _itemsById.ContainsKey(itemId);
    public bool ContainsSourceId(string externalId) => _itemsBySourceId.ContainsKey(externalId);

    public event Action<QueueEvent>? Changed;
}
