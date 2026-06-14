namespace QuikytLoader.Application.DTOs;

public record PlaylistMetadataDto(string PlaylistId, string PlaylistTitle, IReadOnlyList<PlaylistVideoDto> PlaylistVideos);
