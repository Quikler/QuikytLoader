namespace QuikytLoader.Infrastructure.Youtube.ACL.RawModels;

internal sealed record YtDlpVideoRaw(
    string Title,
    string? Channel,
    string Description,
    double DurationSeconds);

internal sealed record YtDlpPlaylistRaw(
    string Title,
    IReadOnlyList<YtDlpPlaylistEntryRaw> Entries);

internal sealed record YtDlpPlaylistEntryRaw(
    string Id,
    string Url,
    string Title,
    string? Channel,
    string Description,
    double DurationSeconds);
