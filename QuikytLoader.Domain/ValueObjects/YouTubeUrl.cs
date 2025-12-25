using QuikytLoader.Domain.Common;

namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Value object representing a YouTube URL with validation logic
/// </summary>
public record YouTubeUrl
{
    public string Value { get; }

    private YouTubeUrl(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a validated YouTubeUrl instance.
    /// </summary>
    public static Result<YouTubeUrl> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Error("YouTube URL cannot be empty");

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return new Error("Invalid URL format");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return new Error("URL must use HTTP or HTTPS");

        if (!IsYouTubeHost(uri))
            return new Error("URL must be from youtube.com or youtu.be");

        return new YouTubeUrl(value);
    }

    private static bool IsYouTubeHost(Uri uri) =>
        uri.Host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase) ||
        uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase);

    public static implicit operator string(YouTubeUrl url) => url.Value;
    public override string ToString() => Value;
}
