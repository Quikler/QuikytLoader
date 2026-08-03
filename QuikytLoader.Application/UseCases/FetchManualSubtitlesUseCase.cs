using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.UseCases;

public class FetchManualSubtitlesUseCase(
    IYoutubeSubtitlesService youtubeSubtitlesService,
    IDownloadQueue queue,
    ITempDirectoryService tempDirectoryService)
{
    public async Task<SubtitleFetchResult> ExecuteAsync(Guid itemId)
    {
        var queueItem = queue.GetItem(itemId);
        if (!queueItem.Subtitles.StartManualSubtitlesLoading())
            return new SubtitleFetchResult.NotAllowed();

        var subtitlesDirectory =
            tempDirectoryService.CreateSubdirectory(queueItem.Source.YoutubeVideoId, "manual-subtitles");

        SubtitleFetchResult? result = null;

        try
        {
            var subtitlesResult = await youtubeSubtitlesService.FetchManualSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                subtitlesDirectory);

            if (!subtitlesResult.IsSuccess)
                return result = new SubtitleFetchResult.Failed(subtitlesResult.Error.Message);

            if (subtitlesResult.Value is not null)
            {
                queueItem.Subtitles.SetManualSubtitles(subtitlesResult.Value);
                return result = new SubtitleFetchResult.Fetched();
            }

            return result = new SubtitleFetchResult.NotFound(
                Errors.Youtube.SubtitlesNotFound().Message);
        }
        catch (OperationCanceledException)
        {
            return result = new SubtitleFetchResult.Canceled(
                Errors.Youtube.SubtitlesFetchCanceled().Message);
        }
        finally
        {
            queueItem.Subtitles.FinishManualSubtitlesLoading(
                result ?? new SubtitleFetchResult.Failed(
                    "Unexpected subtitle fetch error"));
            tempDirectoryService.DeleteSubdirectory(subtitlesDirectory);
        }
    }
}
