using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Finds exising download
/// </summary>
public class FindExistingDownloadUseCase(
    IDownloadHistoryRepository historyRepo,
    IYoutubeExtractorService youtubeExtractorService)
{
    public async Task<Result<DownloadHistoryDto?>> FindAsync(string youtubeUrl)
    {
        var youtubeIdResult = youtubeExtractorService.GetVideoId(youtubeUrl);
        if (!youtubeIdResult.IsSuccess)
            return Result<DownloadHistoryDto?>.Failure(youtubeIdResult.Error);

        return await FindByIdAsync(youtubeIdResult.Value);
    }

    /// <summary>
    /// Looks up a history record directly by YouTube id (skips the yt-dlp extraction call).
    /// </summary>
    public async Task<Result<DownloadHistoryDto?>> FindByIdAsync(string youtubeId)
    {
        var idResult = YouTubeId.Create(youtubeId);
        if (!idResult.IsSuccess)
            return Result<DownloadHistoryDto?>.Failure(idResult.Error);

        var downloadEntity = await historyRepo.GetByIdAsync(idResult.Value);
        if (downloadEntity is null)
            return Result<DownloadHistoryDto?>.Success(null);

        var duplicateResult = new DownloadHistoryDto(downloadEntity.YouTubeId, downloadEntity.VideoTitle, DateTime.Parse(downloadEntity.DownloadedAt));
        return Result<DownloadHistoryDto?>.Success(duplicateResult);
    }

    public async Task<(PlaylistVideoDto PlaylistVideoDto, Result<DownloadHistoryDto?> DuplicateCheck)[]> FindMultipleAsync(IEnumerable<PlaylistVideoDto> playlistVideoDtos)
    {
        var duplicateCheckTasks = playlistVideoDtos
            .Select(async playlistVideoDto => (
                PlaylistVideoDto: playlistVideoDto,
                DuplicateCheck: await FindAsync(playlistVideoDto.Source.Url)
            ));

        return await Task.WhenAll(duplicateCheckTasks);
    }
}
