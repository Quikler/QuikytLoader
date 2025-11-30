namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Value object representing a YouTube video ID (always 11 characters)
/// </summary>
public record YouTubeId
{
    private const int ValidLength = 11;

    public string Value { get; }

    public YouTubeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("YouTube ID cannot be empty", nameof(value));

        if (value.Length != ValidLength)
            throw new ArgumentException($"YouTube ID must be exactly {ValidLength} characters", nameof(value));

        Value = value;
    }

    public static implicit operator string(YouTubeId id) => id.Value;
    public override string ToString() => Value;
}
