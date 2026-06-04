namespace QuikytLoader.Application.DTOs;

public record PlaylistMetadataDto(string PlaylistId, string Title, IReadOnlyList<VideoMetadataDto> PlaylistVideos);
