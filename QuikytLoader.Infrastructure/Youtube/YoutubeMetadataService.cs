using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Mappers;
using QuikytLoader.Infrastructure.Youtube.ACL.Services;

namespace QuikytLoader.Infrastructure.Youtube;

internal sealed class YoutubeMetadataService(IYtDlpAcl ytDlpAcl) : IYoutubeMetadataService
{
    public async Task<Result<VideoMetadata>> GetVideoMetadataAsync(DownloadSource downloadSource, CancellationToken ct)
    {
        var raw = await ytDlpAcl.GetVideoAsync(downloadSource, ct);
        return raw.IsSuccess
            ? raw.Value.ToDomain()
            : raw.Error;
    }

    public async Task<Result<PlaylistMetadata>> GetPlaylistMetadataAsync(DownloadPlaylistSource downloadPlaylistSource, int maxItems, CancellationToken ct)
    {
        var raw = await ytDlpAcl.GetPlaylistAsync(downloadPlaylistSource, maxItems, ct);
        return raw.IsSuccess
            ? raw.Value.ToDomain()
            : raw.Error;
    }
}
