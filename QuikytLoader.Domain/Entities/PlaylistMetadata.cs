namespace QuikytLoader.Domain.Entities;

public record PlaylistMetadata(string PlaylistTitle, IReadOnlyList<PlaylistVideo> PlaylistVideos);
