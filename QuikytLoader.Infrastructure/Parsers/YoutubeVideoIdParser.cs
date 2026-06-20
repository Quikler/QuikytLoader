using System.Text.RegularExpressions;
using QuikytLoader.Application.Interfaces.Parsers;
using QuikytLoader.Domain.Common;
using QuikytLoader.Infrastructure.Constants;

namespace QuikytLoader.Infrastructure.Parsers;

public sealed partial class YoutubeVideoIdParser : IYoutubeVideoIdParser
{
    [GeneratedRegex("^[a-zA-Z0-9_-]{11}$")]
    private static partial Regex YoutubeVideoIdRegex();

    public Result<string> Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<string>.Failure("Youtube URL cannot be empty.");

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return Result<string>.Failure("Invalid Youtube URL.");

        var videoId = ExtractVideoId(uri);

        if (string.IsNullOrWhiteSpace(videoId))
            return Result<string>.Failure("Youtube video ID not found.");

        if (!YoutubeVideoIdRegex().IsMatch(videoId))
            return Result<string>.Failure("Invalid Youtube video ID.");

        return videoId;
    }

    private static string? ExtractVideoId(Uri uri)
    {
        var host = uri.Host.ToLowerInvariant();

        if (!YoutubeConstants.SupportedHosts.Contains(host))
            return null;

        if (host == "youtu.be")
            return uri.AbsolutePath
                .Trim('/')
                .Split('/')
                .FirstOrDefault();

        if (uri.AbsolutePath.TrimEnd('/') == "/watch")
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
