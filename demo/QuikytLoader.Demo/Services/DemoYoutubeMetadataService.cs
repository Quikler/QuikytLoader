using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.Services;

internal sealed class DemoYoutubeMetadataService
    : IYoutubeMetadataService
{
    public Task<Result<VideoMetadata>> GetVideoMetadataAsync(DownloadSource downloadSource) =>
        Task.FromResult(Result<VideoMetadata>.Success(
            CreateVideoMetadata()));

    public Task<Result<PlaylistMetadata>> GetPlaylistMetadataAsync(DownloadPlaylistSource source, int maxItems) =>
        Task.FromResult(Result<PlaylistMetadata>.Success(
            CreatePlaylistMetadata(
                source.YoutubePlaylistId,
                maxItems)));
}
