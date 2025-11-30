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
        // Basic validation - check for youtube.com or youtu.be domains
        return url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
    }

    public static implicit operator string(YouTubeUrl url) => url.Value;
    public override string ToString() => Value;
}
