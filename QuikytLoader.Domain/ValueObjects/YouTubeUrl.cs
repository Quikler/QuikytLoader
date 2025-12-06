namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Value object representing a YouTube URL with validation logic
/// </summary>
public record YouTubeUrl
{
    public string Value { get; }

    public YouTubeUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("YouTube URL cannot be empty", nameof(value));

        if (!IsValidYouTubeUrl(value))
            throw new ArgumentException("Invalid YouTube URL format", nameof(value));

        Value = value;
    }

    private static bool IsValidYouTubeUrl(string url)
    {
        // Validate URL format and ensure it's a legitimate YouTube domain
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Validate scheme (must be http or https)
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        // Validate host (must be youtube.com subdomain or youtu.be)
        return uri.Host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase);
    }

    public static implicit operator string(YouTubeUrl url) => url.Value;
    public override string ToString() => Value;
}
