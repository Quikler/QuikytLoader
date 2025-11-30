using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing a YouTube video
/// </summary>
public class YouTubeVideo
{
    public required YouTubeId Id { get; init; }
    public required string Title { get; init; }
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Gets a sanitized version of the title safe for use as a filename
    /// </summary>
    public string GetSanitizedTitle()
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", Title.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
