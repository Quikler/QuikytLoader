namespace QuikytLoader.Domain.Entities;

public record PlaylistMetadata(string PlaylistId, string PlaylistTitle, IReadOnlyList<PlaylistVideo> PlaylistVideos);
