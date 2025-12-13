using QuikytLoader.Domain.Common;

namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Value object representing a YouTube video ID (always 11 characters)
/// </summary>
public record YouTubeId
{
    private const int ValidLength = 11;

    public string Value { get; }

    private YouTubeId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a YouTubeId instance with validation.
    /// </summary>
    /// <param name="value">The YouTube ID string to validate</param>
    /// <returns>Result containing the YouTubeId or validation error</returns>
    public static Result<YouTubeId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(
                "YouTubeId.Empty",
                "YouTube ID cannot be empty");

        if (value.Length != ValidLength)
            return Error.Validation(
                "YouTubeId.InvalidLength",
                $"YouTube ID must be exactly {ValidLength} characters");

        return new YouTubeId(value);
    }

    /// <summary>
    /// Attempts to create a YouTubeId instance. Useful for performance-sensitive UI scenarios.
    /// </summary>
    /// <param name="value">The YouTube ID string to validate</param>
    /// <param name="youtubeId">The created YouTubeId if successful, null otherwise</param>
    /// <returns>True if the ID is valid and YouTubeId was created, false otherwise</returns>
    public static bool TryCreate(string value, out YouTubeId? youtubeId)
    {
        var result = Create(value);
        youtubeId = result.IsSuccess ? result.Value : null;
        return result.IsSuccess;
    }

    public static implicit operator string(YouTubeId id) => id.Value;
    public override string ToString() => Value;
}
