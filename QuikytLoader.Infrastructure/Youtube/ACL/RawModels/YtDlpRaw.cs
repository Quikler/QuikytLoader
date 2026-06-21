namespace QuikytLoader.Infrastructure.Youtube.ACL.RawModels;

internal sealed record YtDlpVideoRaw(
    string Id,
    string Title,
    string Channel,
    double DurationSeconds);

internal sealed record YtDlpPlaylistRaw(
    string Id,
    string Title,
    IReadOnlyList<YtDlpPlaylistEntryRaw> Entries);

internal sealed record YtDlpPlaylistEntryRaw(
    string Id,
    string Url,
    string Title,
    string Channel,
    double DurationSeconds);

