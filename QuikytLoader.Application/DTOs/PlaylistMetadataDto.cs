namespace QuikytLoader.Application.DTOs;

public record PlaylistMetadataDto(string PlaylistId, string Title, IReadOnlyList<PlaylistEntryDto> Entries);

public record PlaylistEntryDto(
    string VideoId,
    string Url,
    string Title,
    string? Channel,
    string? Duration,
    string? ThumbnailUrl,
    bool IsAvailable,
    string? UnavailableReason);
