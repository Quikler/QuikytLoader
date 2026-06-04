namespace QuikytLoader.Application.DTOs;

public record VideoMetadataDto(
    string VideoId,
    string Title,
    string Channel,
    string Duration,
    string ThumbnailUrl,
    bool IsAvailable,
    string UnavailableReason);
