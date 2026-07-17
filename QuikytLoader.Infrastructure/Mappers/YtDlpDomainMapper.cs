using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Youtube.ACL.RawModels;

namespace QuikytLoader.Infrastructure.Mappers;

internal static class YtDlpDomainMapper
{
    public static VideoMetadata ToDomain(this YtDlpVideoRaw raw) =>
        new(raw.Title, raw.Channel, raw.Description, TimeSpan.FromSeconds(raw.DurationSeconds));

    public static PlaylistMetadata ToDomain(this YtDlpPlaylistRaw raw)
        => new(raw.Title, [.. raw.Entries.Select(MapEntry)]);

    private static PlaylistVideo MapEntry(YtDlpPlaylistEntryRaw e) =>
        new(
            new DownloadSource(e.Url, e.Id),
            new VideoMetadata(e.Title, e.Channel, e.Description, TimeSpan.FromSeconds(e.DurationSeconds))
        );
}
