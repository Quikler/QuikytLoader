using System.Diagnostics;
using QuikytLoader.Application.Interfaces.LanguageIdentification;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Application.UseCases;

public class FetchAutoSubtitlesUseCase(
    IYoutubeSubtitlesService youtubeSubtitlesService,
    IYoutubeMetadataService youtubeMetadataService,
    IDownloadQueue queue,
    IUserSettings userSettings,
    ILanguageIdentifier languageIdentifier,
    ITempDirectoryService tempDirectoryService)
{
    public async Task<SubtitleFetchResult> ExecuteAsync(
        Guid itemId,
        Language? language = null)
    {
        var queueItem = queue.GetItem(itemId);
        if (!queueItem.Subtitles.StartAutoSubtitlesLoading())
            return new SubtitleFetchResult.NotAllowed();

        var subtitlesDirectory =
            tempDirectoryService.CreateSubdirectory(queueItem.Source.YoutubeVideoId, "auto-subtitles");

        SubtitleFetchResult? result = null;

        var didAutoSubtitlesOptionChange = false;
        void onSettingsChanged(UserSettingsChangedEventArgs args)
        {
            if (args.OldSettings.AutoSubtitlesOption == args.NewSettings.AutoSubtitlesOption) return;

            didAutoSubtitlesOptionChange = true;
            youtubeSubtitlesService.CancelSubtitlesFetching(itemId);
        }

        try
        {
            userSettings.Changed += onSettingsChanged;

            var isExplicitLanguageSelection = language is not null;
            var autoSubtitlesOption = userSettings.Current.AutoSubtitlesOption;

            if (language is null)
            {
                switch (autoSubtitlesOption)
                {
                    case AutoSubtitlesOption.ManualLanguageSelection:
                        return result = new SubtitleFetchResult.ActionRequired(
                            "Please select video language",
                            null,
                            SubtitleActionRequired.LanguageSelection,
                            userSettings.Current.AutoSubtitlesOption);

                    case AutoSubtitlesOption.AutoLanguageDetection:
                        var videoMetadata = queueItem.Metadata;
                        if (videoMetadata is null)
                        {
                            var videoMetadataResult = await youtubeMetadataService.GetVideoMetadataAsync(queueItem.Source);
                            if (!videoMetadataResult.IsSuccess)
                                return result = new SubtitleFetchResult.Failed(
                                    videoMetadataResult.Error.Message);
                            videoMetadata = videoMetadataResult.Value;
                        }
                        language =
                            languageIdentifier.Identify($"{videoMetadata.Title}\n{videoMetadata.Description}");
                        break;

                    case AutoSubtitlesOption.FallbackToEnglishLanguage:
                        language = Language.English;
                        break;

                    default: throw new UnreachableException();
                }
            }

            if (queueItem.Subtitles.ExistWithLanguage(language.Value.Iso6391Code))
                return result = new SubtitleFetchResult.ActionRequired(
                    $"Subtitles for '{language.Value.DisplayName}' language already fetched, try other languages",
                    null,
                    SubtitleActionRequired.LanguageSelection,
                    autoSubtitlesOption);

            var subtitlesResult = await youtubeSubtitlesService.FetchAutoSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                subtitlesDirectory,
                language.Value.Iso6391Code);

            if (!subtitlesResult.IsSuccess)
                return result = new SubtitleFetchResult.ActionRequired(
                    $"Failed to fetch auto subtitles for '{language.Value.DisplayName}', please select another language and try again",
                    subtitlesResult.Error.Message,
                    SubtitleActionRequired.LanguageSelection,
                    autoSubtitlesOption,
                    true);

            if (subtitlesResult.Value is not null)
            {
                queueItem.Subtitles.SetAutoSubtitles(subtitlesResult.Value);
                return result = new SubtitleFetchResult.Fetched(
                    new SubtitleFetchResult.ActionRequired(
                        $"Auto subtitles were fetched for '{language.Value.DisplayName}', you can try other languages",
                        null,
                        SubtitleActionRequired.LanguageSelection,
                        autoSubtitlesOption));
            }

            return result = new SubtitleFetchResult.ActionRequired(
                $"Auto subtitles for '{language.Value.DisplayName}' not found, please select another language and try again",
                null,
                SubtitleActionRequired.LanguageSelection,
                autoSubtitlesOption,
                true);
        }
        catch (OperationCanceledException)
        {
            if (didAutoSubtitlesOptionChange)
            {
                return result = new SubtitleFetchResult.ActionRequired(
                    "Auto Subtitles Option settings were changed, please click refresh",
                    null,
                    SubtitleActionRequired.RefreshDueToSettingsChange,
                    null);
            }

            return result = queueItem.Subtitles.LastSeenAutoSubtitleFetchResult switch
            {
                SubtitleFetchResult.ActionRequired r
                    when r.CreatedWithOption != userSettings.Current.AutoSubtitlesOption
                        || r.CreatedWithOption is null =>
                    new SubtitleFetchResult.Canceled(Errors.Youtube.AutoSubtitlesFetchCanceled().Message),
                SubtitleFetchResult.ActionRequired r => r,
                SubtitleFetchResult.Fetched r
                    when r.Action is not null => r.Action,
                _ => new SubtitleFetchResult.Canceled(Errors.Youtube.AutoSubtitlesFetchCanceled().Message)
            };
        }
        finally
        {
            queueItem.Subtitles.FinishAutoSubtitlesLoading(
                result ?? new SubtitleFetchResult.Failed(
                    "Unexpected subtitle fetch error"));
            tempDirectoryService.DeleteSubdirectory(subtitlesDirectory);
            userSettings.Changed -= onSettingsChanged;
        }
    }
}
