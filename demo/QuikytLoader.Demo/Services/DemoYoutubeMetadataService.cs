using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Demo.Seed;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.Services;

internal sealed class DemoYoutubeMetadataService(
    DemoDataSeed dataSeed)
    : IYoutubeMetadataService
{
    public Task<Result<VideoMetadata>> GetVideoMetadataAsync(DownloadSource downloadSource) =>
        Task.FromResult(Result<VideoMetadata>.Success(
            dataSeed.CreateVideo(downloadSource.YoutubeVideoId)));

    public Task<Result<PlaylistMetadata>> GetPlaylistMetadataAsync(DownloadPlaylistSource source, int maxItems) =>
        Task.FromResult(Result<PlaylistMetadata>.Success(
            dataSeed.CreatePlaylist(
                source.YoutubePlaylistId,
                maxItems)));
}
