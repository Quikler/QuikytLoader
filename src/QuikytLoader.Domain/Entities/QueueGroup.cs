namespace QuikytLoader.Domain.Entities;

public sealed class QueueGroup(
    DownloadPlaylistSource source,
    string title,
    IReadOnlyList<Guid> itemIds)
{
    public Guid Id { get; } = Guid.NewGuid();

    public DownloadPlaylistSource Source { get; } = source;

    public string Title { get; } = title;

    public IReadOnlyList<Guid> ItemIds { get; } = itemIds;
}
