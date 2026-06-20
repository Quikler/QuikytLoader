using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Youtube.ACL.RawModels;

namespace QuikytLoader.Infrastructure.Mappers;

internal static class YtDlpDomainMapper
{
    public static VideoMetadata ToDomain(this YtDlpVideoRaw raw)
    {
        var (available, reason) = MapAvailability(raw.Availability);

        return new VideoMetadata(
            raw.Id,
            raw.Title,
            raw.Channel,
            TimeSpan.FromSeconds(raw.DurationSeconds),
            raw.ThumbnailUrl,
            available,
            reason);
    }

    public static PlaylistMetadata ToDomain(this YtDlpPlaylistRaw raw)
        => new(raw.Id, raw.Title, [.. raw.Entries.Select(MapEntry)]);

    private static PlaylistVideo MapEntry(YtDlpPlaylistEntryRaw e)
    {
        var (available, reason) = MapAvailability(e.Availability);

        return new PlaylistVideo(
            new DownloadSource(e.Url, e.Id),
            new VideoMetadata(
                e.Id,
                e.Title,
                e.Channel,
                TimeSpan.FromSeconds(e.DurationSeconds),
                e.Thumbnails.Last().Url,
                available,
                reason));
    }

    private static (bool isAvailable, string unavailableReason) MapAvailability(string? availability) =>
        availability switch
        {
            null or "" or "public" or "unlisted" => (true, string.Empty),
            "private" => (false, "Private video"),
            "premium_only" => (false, "Premium only"),
            "subscriber_only" => (false, "Members only"),
            "needs_auth" => (false, "Sign-in required"),
            _ => (false, availability ?? "Unknown")
        };
}
