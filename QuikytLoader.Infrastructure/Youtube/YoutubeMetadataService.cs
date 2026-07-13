using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Mappers;
using QuikytLoader.Infrastructure.Youtube.ACL.Services;

namespace QuikytLoader.Infrastructure.Youtube;

internal sealed class YoutubeMetadataService(IYtDlpAcl ytDlpAcl) : IYoutubeMetadataService
{
    private readonly Dictionary<DownloadSource, Task<Result<ACL.RawModels.YtDlpVideoRaw>>> _videoMetadataTasks = [];

    public async Task<Result<VideoMetadata>> GetVideoMetadataAsync(
        DownloadSource downloadSource,
        CancellationToken ct)
    {
        Task<Result<ACL.RawModels.YtDlpVideoRaw>> rawResultTask =
            _videoMetadataTasks.TryGetValue(downloadSource, out var task)
                ? task
                : _videoMetadataTasks[downloadSource] = ytDlpAcl.GetVideoAsync(downloadSource, ct);

        var rawResult = await rawResultTask;
        return rawResult.IsSuccess
            ? rawResult.Value.ToDomain()
            : rawResult.Error;
    }

    public async Task<Result<PlaylistMetadata>> GetPlaylistMetadataAsync(
        DownloadPlaylistSource downloadPlaylistSource,
        int maxItems,
        CancellationToken ct)
    {
        var raw = await ytDlpAcl.GetPlaylistAsync(downloadPlaylistSource, maxItems, ct);
        return raw.IsSuccess
            ? raw.Value.ToDomain()
            : raw.Error;
    }
}
