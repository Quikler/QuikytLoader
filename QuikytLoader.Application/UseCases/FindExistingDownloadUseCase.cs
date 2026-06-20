using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Finds exising download
/// </summary>
public class FindExistingDownloadUseCase(IDownloadHistoryRepository historyRepo)
{
    /// <summary>
    /// Looks up a history record directly by Youtube id (skips the yt-dlp extraction call).
    /// </summary>
    public async Task<Result<DownloadHistoryDto?>> FindByIdAsync(string youtubeVideoId)
    {
        var downloadEntity = await historyRepo.GetByYoutubeVideoIdAsync(youtubeVideoId);
        if (downloadEntity is null)
            return Result<DownloadHistoryDto?>.Success(null);

        var duplicateResult = new DownloadHistoryDto(downloadEntity.YoutubeVideoId, downloadEntity.VideoTitle, DateTime.Parse(downloadEntity.DownloadedAt));
        return Result<DownloadHistoryDto?>.Success(duplicateResult);
    }

    public async Task<(PlaylistVideo PlaylistVideo, Result<DownloadHistoryDto?> DuplicateCheck)[]> FindMultipleAsync(IEnumerable<PlaylistVideo> playlistVideoDtos)
    {
        var duplicateCheckTasks = playlistVideoDtos
            .Select(async playlistVideo => (
                PlaylistVideo: playlistVideo,
                DuplicateCheck: await FindByIdAsync(playlistVideo.Source.YoutubeVideoId)
            ));

        return await Task.WhenAll(duplicateCheckTasks);
    }
}
