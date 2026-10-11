using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.Common;

internal static class DemoFactories
{
    public static VideoMetadata CreateVideoMetadata() => new(
        RandomTitle,
        RandomChannel,
        RandomDescription,
        RandomDuration
    );

    public static PlaylistMetadata CreatePlaylistMetadata(
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
                    CreateVideoMetadata());
            })
            .ToList();

        return new($"Playlist-{playlistId}", videos);
    }

    public static IReadOnlyDictionary<string, string> CreateSubtitles()
    {
        var result = new Dictionary<string, string>();

        foreach (var (language, sampleSubtitles) in SubtitleSamples)
        {
            // 60% chance subtitles exist for this language
            if (Random.Shared.NextDouble() < 0.6)
            {
                result[language] = string.Join(
                    // Join subtitles row with space or with new line for UI testing
                    Random.Shared.Next(0, 2) == 0 ? ' ' : '\n',
                    Enumerable.Range(0, Random.Shared.Next(6, 66))
                        .Select(_ => sampleSubtitles[Random.Shared.Next(sampleSubtitles.Length)]));
            }
        }

        return result;
    }

    public static DownloadResultEntity CreateDownloadResultEntity(
        DownloadSource downloadSource,
        string downloadDirectory,
        string? customTitle = null)
    {
        var title = customTitle ?? RandomTitle;
        return new(
            downloadSource.YoutubeVideoId,
            Path.Combine(downloadDirectory, $"{title}.mp3"),
            Path.Combine(downloadDirectory, $"{title}.jpeg"));
    }

    public static DownloadHistoryEntity CreateDownloadHistoryEntity(
        string youtubeVideoId)
            => new(youtubeVideoId,
                RandomTitle,
                DateTime.UtcNow.ToString("o"));

    public static PreparedMedia CreatePreparedMedia(
        string tempMp3FilePath,
        string tempThumbnailFilePath)
            => new(
                tempMp3FilePath,
                tempThumbnailFilePath,
                tempMp3FilePath,
                tempThumbnailFilePath);

    public static PreparedMedia CreatePreparedMediaWithTitle(
        string mediaDirectory,
        string tempMp3FilePath,
        string tempThumbnailFilePath,
        string sanitizedCustomTitle)
            => new(
                Path.Combine(mediaDirectory, Path.Combine(sanitizedCustomTitle, Path.GetExtension(tempMp3FilePath))),
                Path.Combine(mediaDirectory, Path.Combine(sanitizedCustomTitle, Path.GetExtension(tempThumbnailFilePath))),
                tempMp3FilePath,
                tempThumbnailFilePath);

    private static string RandomVideoId() => Guid.NewGuid().ToString("N")[..11];
}
