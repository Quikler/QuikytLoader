using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.Services;

internal sealed class DemoDownloadHistoryRepository
    : IDownloadHistoryRepository
{
    public Task<DownloadHistoryEntity?> GetByYoutubeVideoIdAsync(string youtubeVideoId)
    {
        // 80% chance that history doesn't exist
        return Random.Shared.NextDouble() < 0.8
            ? Task.FromResult<DownloadHistoryEntity?>(null)
            : Task.FromResult<DownloadHistoryEntity?>(CreateDownloadHistoryEntity(youtubeVideoId));
    }

    public Task UpsertAsync(DownloadHistoryEntity downloadEntity)
        => Task.CompletedTask;
}
