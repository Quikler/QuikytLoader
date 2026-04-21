using System.Web;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Value object representing a YouTube playlist URL (any URL containing a list= parameter).
/// </summary>
public record YouTubePlaylistUrl
{
    public string Value { get; }
    public string PlaylistId { get; }

    private YouTubePlaylistUrl(string value, string playlistId)
    {
        Value = value;
        PlaylistId = playlistId;
    }

    /// <summary>
    /// Creates a validated YouTubePlaylistUrl. URL must be a YouTube URL containing a list= query parameter.
    /// </summary>
    public static Result<YouTubePlaylistUrl> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error("Playlist URL cannot be empty");

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return new Error("Invalid URL format");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return new Error("URL must use HTTP or HTTPS");

        if (!IsYouTubeHost(uri))
            return new Error("URL must be from youtube.com or youtu.be");

        var playlistId = HttpUtility.ParseQueryString(uri.Query).Get("list");
        if (string.IsNullOrWhiteSpace(playlistId))
            return new Error("URL does not contain a playlist (missing list= parameter)");

        return new YouTubePlaylistUrl(value, playlistId);
    }

    /// <summary>
    /// Checks whether a URL contains a list= query parameter (without full validation).
    /// </summary>
    public static bool HasPlaylistParam(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        return !string.IsNullOrWhiteSpace(HttpUtility.ParseQueryString(uri.Query).Get("list"));
    }

    private static bool IsYouTubeHost(Uri uri) =>
        uri.Host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase) ||
        uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase);

    public static implicit operator string(YouTubePlaylistUrl url) => url.Value;
    public override string ToString() => Value;
}
