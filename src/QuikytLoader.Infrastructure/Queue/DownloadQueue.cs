using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Infrastructure.Queue;

public sealed class DownloadQueue : IDownloadQueue
{
    private readonly Dictionary<Guid, QueueItem> _itemsById = [];
    private readonly Dictionary<DownloadSource, QueueItem> _itemsBySource = [];

    private readonly Dictionary<DownloadPlaylistSource, QueueGroup> _groupsBySource = [];

    public int ItemsCount => _itemsById.Values.Count;

    public void EnqueueItem(QueueItem item)
    {
        _itemsById[item.Id] = item;
        _itemsBySource[item.Source] = item;

        Changed?.Invoke(new QueueEvent.ItemAdded(item));
    }

    public void EnqueueGroup(QueueGroup group, IEnumerable<QueueItem> items)
    {
        _groupsBySource[group.Source] = group;

        foreach (var item in items)
        {
            _itemsById[item.Id] = item;
            _itemsBySource[item.Source] = item;
        }

        Changed?.Invoke(new QueueEvent.GroupAdded(group));
    }

    public QueueItem GetItem(Guid id)
        => _itemsById[id];

    public void UpdateItem(Guid itemId)
        => Changed?.Invoke(new QueueEvent.ItemUpdated(itemId));

    public bool ContainsItemSource(DownloadSource source)
        => _itemsBySource.ContainsKey(source);

    public bool ContainsGroupSource(DownloadPlaylistSource source)
        => _groupsBySource.ContainsKey(source);

    public event Action<QueueEvent>? Changed;
}
