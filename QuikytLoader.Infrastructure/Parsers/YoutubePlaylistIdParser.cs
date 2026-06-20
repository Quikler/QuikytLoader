using QuikytLoader.Application.Interfaces.Parsers;
using QuikytLoader.Domain.Common;
using QuikytLoader.Infrastructure.Constants;

namespace QuikytLoader.Infrastructure.Parsers;

public sealed class YoutubePlaylistIdParser : IYoutubePlaylistIdParser
{
    public Result<string> Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<string>.Failure("Youtube URL cannot be empty.");

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return Result<string>.Failure("Invalid Youtube URL.");

        var host = uri.Host.ToLowerInvariant();

        if (!YoutubeConstants.SupportedHosts.Contains(host))
            return Result<string>.Failure("Unsupported Youtube URL.");

        var playlistId = System.Web.HttpUtility.ParseQueryString(uri.Query)["list"];
        return string.IsNullOrWhiteSpace(playlistId)
            ? Result<string>.Failure("Youtube playlist ID not found.")
            : playlistId;
    }
}
