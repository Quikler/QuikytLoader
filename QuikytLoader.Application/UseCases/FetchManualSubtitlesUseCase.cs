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

        if (!queueItem.StartManualSubtitlesLoading())
            return new SubtitleFetchResult.NotAllowed();

        var subtitlesDirectory =
            tempDirectoryService.CreateSubdirectory(queueItem.Source.YoutubeVideoId, "subtitles");

        SubtitleFetchResult? result = null;

        try
        {
            var subtitlesResult = await youtubeSubtitlesService.FetchManualSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                subtitlesDirectory);

            if (!subtitlesResult.IsSuccess)
                return result = new SubtitleFetchResult.Failed(subtitlesResult.Error.Message, true);

            if (subtitlesResult.Value is not null)
            {
                queueItem.SetManualSubtitles(subtitlesResult.Value);
                return result = new SubtitleFetchResult.Fetched();
            }

            return result = new SubtitleFetchResult.NotFound(
                Errors.Youtube.SubtitlesNotFound().Message,
                false);
        }
        catch (OperationCanceledException)
        {
            return result = new SubtitleFetchResult.Canceled(
                Errors.Youtube.SubtitlesFetchCanceled().Message,
                true);
        }
        finally
        {
            queueItem.FinishManualSubtitlesLoading(
                result ?? new SubtitleFetchResult.Failed(
                    "Unexpected subtitle fetch error",
                    true));
            tempDirectoryService.DeleteSubdirectory(subtitlesDirectory);
        }
    }
}
