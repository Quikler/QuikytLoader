using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Services;

public interface IYoutubeMetadataService
{
    Task<Result<VideoMetadata>> GetVideoMetadataAsync(DownloadSource downloadSource, CancellationToken ct);

    Task<Result<PlaylistMetadata>> GetPlaylistMetadataAsync(DownloadPlaylistSource downloadPlaylistSource, int maxItems, CancellationToken ct);
}
