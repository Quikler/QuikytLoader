namespace QuikytLoader.Domain.Entities;

public sealed record QueueGroup(string Id, string Title, IReadOnlyList<Guid> ItemIds);
