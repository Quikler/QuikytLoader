using QuikytLoader.Domain.Common;

namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Value object representing a YouTube video ID (always 11 characters)
/// </summary>
public record YouTubeId
{
    private const int ValidLength = 11;

    public string Id { get; }

    private YouTubeId(string value)
    {
        Id = value;
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

    public static implicit operator string(YouTubeId id) => id.Id;
    public override string ToString() => Id;
}
