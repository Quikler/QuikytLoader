using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

public interface IFetchManualSubtitlesUseCase
{
    Task<SubtitlesFetchResult> ExecuteAsync(
        Guid itemId);
}

public class FetchManualSubtitlesUseCase(
    IYoutubeSubtitlesService youtubeSubtitlesService,
    IDownloadQueue queue,
    ITempDirectoryService tempDirectoryService)
        : IFetchManualSubtitlesUseCase
{
    public async Task<SubtitlesFetchResult> ExecuteAsync(
        Guid itemId)
    {
        var queueItem = queue.GetItem(itemId);
        if (!queueItem.Subtitles.StartManualSubtitlesLoading())
            return new SubtitlesFetchResult.NotAllowed();

        var subtitlesDirectory =
            tempDirectoryService.CreateSubdirectory(queueItem.Source.YoutubeVideoId, "manual-subtitles");

        SubtitlesFetchResult? result = null;

        try
        {
            var subtitlesResult = await youtubeSubtitlesService.FetchManualSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                subtitlesDirectory);

            if (!subtitlesResult.IsSuccess)
                return result = new SubtitlesFetchResult.Failed(subtitlesResult.Error.Message);

            if (subtitlesResult.Value is not null)
            {
                queueItem.Subtitles.SetManualSubtitles(subtitlesResult.Value);
                return result = new SubtitlesFetchResult.Fetched();
            }

            return result = new SubtitlesFetchResult.NotFound(
                Errors.Youtube.SubtitlesNotFound().Message);
        }
        catch (OperationCanceledException)
        {
            return result = new SubtitlesFetchResult.Canceled(
                Errors.Youtube.SubtitlesFetchCanceled().Message);
        }
        finally
        {
            queueItem.Subtitles.FinishManualSubtitlesLoading(
                result ?? new SubtitlesFetchResult.Failed(
                    "Unexpected subtitles fetch error"));
            tempDirectoryService.DeleteSubdirectory(subtitlesDirectory);
        }
    }
}
