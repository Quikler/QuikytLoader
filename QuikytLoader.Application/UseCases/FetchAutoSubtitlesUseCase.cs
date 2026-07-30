using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Application.UseCases;

public class FetchAutoSubtitlesUseCase(IYoutubeSubtitlesService youtubeSubtitlesService, IDownloadQueue queue, IUserSettings userSettings, ITempDirectoryService tempDirectoryService)
{
    public async Task<SubtitleFetchResult> ExecuteAsync(
        Guid itemId,
        string? manuallySelectedLanguageForAutoSubtitles = null)
    {
        var queueItem = queue.GetItem(itemId);

        if (!queueItem.StartAutoSubtitlesLoading())
            return new SubtitleFetchResult.NotAllowed();

        var subtitlesDirectory =
            tempDirectoryService.CreateSubdirectory(queueItem.Source.YoutubeVideoId, "auto-subtitles");

        SubtitleFetchResult? result = null;

        try
        {
            var autoSubtitlesOption = userSettings.Current.AutoSubtitlesOption;
            switch (autoSubtitlesOption)
            {
                case AutoSubtitlesOption.ManualLanguageSelection
                    when manuallySelectedLanguageForAutoSubtitles is null:
                    return result = new SubtitleFetchResult.ActionRequired(
                        "Please select video language",
                        null,
                        SubtitleActionRequired.LanguageSelection);

                case AutoSubtitlesOption.AutoLanguageDetection:
                    manuallySelectedLanguageForAutoSubtitles = string.Empty;
                    break;

                case AutoSubtitlesOption.FallbackToEnglishLanguage:
                    manuallySelectedLanguageForAutoSubtitles = "en";
                    break;
            }

            var subtitlesResult = await youtubeSubtitlesService.FetchAutoSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                queueItem.Metadata,
                subtitlesDirectory,
                manuallySelectedLanguageForAutoSubtitles);

            if (!subtitlesResult.IsSuccess)
            {
                if (autoSubtitlesOption == AutoSubtitlesOption.ManualLanguageSelection)
                    return result = new SubtitleFetchResult.ActionRequired(
                        "Failed to fetch auto subtitles - please verify your language and try again",
                        subtitlesResult.Error.Message,
                        SubtitleActionRequired.LanguageSelection,
                        true);

                if (autoSubtitlesOption == AutoSubtitlesOption.AutoLanguageDetection)
                    return result = new SubtitleFetchResult.ActionRequired(
                        "Failed to fetch auto subtitles with auto language detection, please try again or change Auto Subtitles option in settings",
                        subtitlesResult.Error.Message,
                        SubtitleActionRequired.ChangeAutoSubtitlesOption,
                        true);
            }

            if (subtitlesResult.Value is not null)
            {
                queueItem.SetAutoSubtitles(subtitlesResult.Value);
                return result = new SubtitleFetchResult.Fetched();
            }

            return result = new SubtitleFetchResult.NotFound(
                Errors.Youtube.SubtitlesNotFound().Message,
                false);
        }
        catch (OperationCanceledException)
        {
            return result = queueItem.LastSeenAutoSubtitleFetchResult switch
            {
                SubtitleFetchResult.ActionRequired r => r,
                _ => new SubtitleFetchResult.Canceled(Errors.Youtube.SubtitlesFetchCanceled().Message, true)
            };
        }
        finally
        {
            queueItem.FinishAutoSubtitlesLoading(
                result ?? new SubtitleFetchResult.Failed(
                    "Unexpected subtitle fetch error",
                    true));
            tempDirectoryService.DeleteSubdirectory(subtitlesDirectory);
        }
    }
}
