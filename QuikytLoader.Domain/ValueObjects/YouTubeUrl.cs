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
    /// Creates a YouTubeUrl instance with validation.
    /// </summary>
    /// <param name="value">The YouTube URL string to validate</param>
    /// <returns>Result containing the YouTubeUrl or validation error</returns>
    public static Result<YouTubeUrl> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(
                "YouTubeUrl.Empty",
                "YouTube URL cannot be empty");

        if (!IsValidYouTubeUrl(value))
            return Error.Validation(
                "YouTubeUrl.InvalidFormat",
                "Invalid YouTube URL format");

        return new YouTubeUrl(value);
    }

    /// <summary>
    /// Attempts to create a YouTubeUrl instance. Useful for performance-sensitive UI scenarios.
    /// </summary>
    /// <param name="value">The YouTube URL string to validate</param>
    /// <param name="youtubeUrl">The created YouTubeUrl if successful, null otherwise</param>
    /// <returns>True if the URL is valid and YouTubeUrl was created, false otherwise</returns>
    public static bool TryCreate(string value, out YouTubeUrl? youtubeUrl)
    {
        var result = Create(value);
        youtubeUrl = result.IsSuccess ? result.Value : null;
        return result.IsSuccess;
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
