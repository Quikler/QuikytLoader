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
}
