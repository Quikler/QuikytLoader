using QuikytLoader.Application.Interfaces.Parsers;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Infrastructure.Parsers;

public sealed partial class YoutubeVideoIdParser : IYoutubeVideoIdParser
{
    private static readonly HashSet<string> SupportedHosts =
    [
        "youtube.com",
        "www.youtube.com",
        "m.youtube.com",
        "youtu.be"
    ];

    public Result<string> Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<string>.Failure("Youtube URL cannot be empty.");

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return Result<string>.Failure("Invalid Youtube URL.");

        var videoId = ExtractVideoId(uri);

        if (string.IsNullOrWhiteSpace(videoId))
            return Result<string>.Failure("Youtube video ID not found.");

        if (videoId.Length != 11)
            return Result<string>.Failure("Invalid Youtube video ID.");

        return videoId;
    }

    private static string? ExtractVideoId(Uri uri)
    {
        var host = uri.Host.ToLowerInvariant();

        if (!SupportedHosts.Contains(host))
            return null;

        if (host == "youtu.be")
            return uri.AbsolutePath
                .Trim('/')
                .Split('/')
                .FirstOrDefault();

        if (uri.AbsolutePath == "/watch")
            return System.Web.HttpUtility.ParseQueryString(uri.Query)["v"];

        var segments = uri.AbsolutePath
            .Trim('/')
            .Split('/');

        if (segments.Length < 2)
            return null;

        return segments[0] == "embed"
            || segments[0] == "v"
            || segments[0] == "shorts"
            || segments[0] == "live"
                ? segments[1]
                : null;
    }
}
