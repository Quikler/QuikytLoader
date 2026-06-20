namespace QuikytLoader.Infrastructure.Youtube.ACL.RawModels;

internal sealed record YtDlpVideoRaw(
    string Id,
    string Title,
    string Channel,
    double DurationSeconds,
    string ThumbnailUrl,
    string? Availability);

internal sealed record YtDlpPlaylistRaw(
    string Id,
    string Title,
    IReadOnlyList<YtDlpPlaylistEntryRaw> Entries);

internal sealed record YtDlpPlaylistEntryRaw(
    string Id,
    string Title,
    string Channel,
    double DurationSeconds,
    string? Availability,
    IReadOnlyList<ThumbnailRaw> Thumbnails,
    string Url);

internal sealed record ThumbnailRaw(string Url);
