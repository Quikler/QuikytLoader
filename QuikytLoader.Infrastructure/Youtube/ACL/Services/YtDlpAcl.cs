using System.Text.Json;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Persistence.Json;
using QuikytLoader.Infrastructure.Youtube.ACL.RawModels;
using QuikytLoader.Infrastructure.Youtube.YtDlp;

namespace QuikytLoader.Infrastructure.Youtube.ACL.Services;

internal sealed class YtDlpAcl(IYtDlpProcessClient ytDlpProcessClient) : IYtDlpAcl
{
    private const int ExpectedLinesOutputForVideo = 4;

    public async Task<Result<YtDlpVideoRaw>> GetVideoAsync(
        DownloadSource downloadSource,
        CancellationToken ct)
    {
        var args = new[]
        {
            "--quiet",
            "--skip-download",
            "--no-playlist",
            "--print", "id",
            "--print", "title",
            "--print", "channel",
            "--print", "duration",
            "--", downloadSource.YoutubeVideoId
        };

        var outputResult = await ytDlpProcessClient.RunCaptureAsync(args, ct);
        if (!outputResult.IsSuccess)
            return outputResult.Error;

        var lines = outputResult.Value.Split(
            '\n',
            StringSplitOptions.TrimEntries);

        if (lines.Length < ExpectedLinesOutputForVideo)
        {
            return Errors.Youtube.YtDlpMalformed(
                ExpectedLinesOutputForVideo,
                lines.Length);
        }

        return Result<YtDlpVideoRaw>.Success(
            new YtDlpVideoRaw(
                lines[0],
                lines[1],
                lines[2],
                double.Parse(lines[3], System.Globalization.CultureInfo.InvariantCulture)));
    }

    public async Task<Result<YtDlpPlaylistRaw>> GetPlaylistAsync(
        DownloadPlaylistSource downloadPlaylistSource,
        int maxItems,
        CancellationToken ct)
    {
        var args = new[]
        {
            "--quiet",
            "--flat-playlist",
            "--playlist-items", $"1:{maxItems}",
            "--dump-single-json",
            "--", downloadPlaylistSource.YoutubePlaylistUrl
        };

        var outputResult = await ytDlpProcessClient.RunCaptureAsync(args, ct);

        if (!outputResult.IsSuccess)
            return outputResult.Error;

        var parsed = JsonSerializer.Deserialize(
            outputResult.Value,
            AppJsonSerializerContext.Default.YtDlpPlaylistJson)!;

        return Result<YtDlpPlaylistRaw>.Success(
            new YtDlpPlaylistRaw(
                parsed.Id,
                parsed.Title,
                parsed.Entries
                    .Select(e =>
                        new YtDlpPlaylistEntryRaw(
                            e.Id,
                            e.Url,
                            e.Title,
                            e.Channel,
                            e.Duration))
                    .ToList()));
    }

    public Task<Result> DownloadAudioAsync(
        DownloadSource downloadSource,
        string downloadDirectory,
        string? fileName,
        Action<string>? onOutputLine,
        Action<string>? onErrorLine,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "%(title)s";

        var outputPath = Path.Combine(
            downloadDirectory,
            fileName);

        var args = new[]
        {
            "--extract-audio",
            "--audio-format", "mp3",
            "--audio-quality", "0",
            "--output", $"{outputPath}.%(ext)s",
            "--no-playlist",
            "--add-metadata",
            "--embed-thumbnail",
            "--write-thumbnail",
            "--convert-thumbnails", "jpg",

            "--parse-metadata", $"{fileName}:%(meta_title)s",
            "--parse-metadata", "%(uploader)s:%(meta_artist)s",
            "--parse-metadata", "%(uploader)s:%(meta_album_artist)s",
            "--parse-metadata", "%(channel)s:%(meta_album)s",
            "--parse-metadata", "%(upload_date>%Y)s:%(meta_date)s",
            "--parse-metadata", "%(creator)s:%(meta_composer)s",
            "--parse-metadata", "%(uploader)s:%(meta_performer)s",
            "--parse-metadata", "%(description)s:%(meta_comment)s",
            "--parse-metadata", "%(channel)s:%(meta_publisher)s",
            "--parse-metadata", "%(webpage_url)s:%(meta_purl)s",
            "--parse-metadata", "%(genre)s:%(meta_genre)s",

            "--progress",
            "--", downloadSource.YoutubeVideoId
        };

        return ytDlpProcessClient.RunStreamingAsync(
            args,
            onOutputLine,
            onErrorLine,
            ct);
    }
}
