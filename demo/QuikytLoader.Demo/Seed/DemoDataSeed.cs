using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.Seed;

internal sealed class DemoDataSeed
{
    private static readonly string[] Titles =
    [
        "Short video",
        "A normal length video title for testing UI",
        "This is a very long video title that should test wrapping and overflow behavior in the UI",
        "🎵 Music video with emoji",
        "Video with special characters: тест видео 日本語"
    ];

    private static readonly string?[] Channels =
    [
        "Channel",
        "VeryLongChannelNameThatMayBreakTheLayout",
        "Super Puper Very Long Channel Name That May Break The Layout But I Hope It Won't",
        "Short",
        "Demo Creator",
        null
    ];

    private static readonly TimeSpan[] Durations =
    [
        TimeSpan.FromSeconds(15),
        TimeSpan.FromMinutes(3),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(2),
        TimeSpan.Zero
    ];

    public VideoMetadata CreateVideo(string id) => new(
        id,
        Titles[Random.Shared.Next(Titles.Length)],
        Channels[Random.Shared.Next(Channels.Length)],
        Durations[Random.Shared.Next(Durations.Length)]
    );

    public PlaylistMetadata CreatePlaylist(
        string playlistId,
        int count)
    {
        var videos = Enumerable.Range(1, count)
            .Select(i =>
            {
                var id = RandomVideoId();

                return new PlaylistVideo(
                    new DownloadSource(
                        $"https://youtube.com/watch?v={id}",
                        id),
                    CreateVideo(id));
            })
            .ToList();

        return new($"Playlist-{playlistId}", videos);
    }

    private static string RandomVideoId() => Guid.NewGuid().ToString("N")[..11];
}
