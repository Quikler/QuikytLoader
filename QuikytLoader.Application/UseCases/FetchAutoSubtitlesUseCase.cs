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
    public async Task<SubtitlesFetchResult> ExecuteAsync(
        Guid itemId,
        Language? language = null)
    {
        var queueItem = queue.GetItem(itemId);
        if (!queueItem.Subtitles.StartAutoSubtitlesLoading())
            return new SubtitlesFetchResult.NotAllowed();

        var subtitlesDirectory =
            tempDirectoryService.CreateSubdirectory(queueItem.Source.YoutubeVideoId, "auto-subtitles");

        SubtitlesFetchResult? result = null;

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
                        return result = new SubtitlesFetchResult.ActionRequired(
                            "Please select video language",
                            null,
                            SubtitlesActionRequired.LanguageSelection,
                            userSettings.Current.AutoSubtitlesOption);

                    case AutoSubtitlesOption.AutoLanguageDetection:
                        var videoMetadata = queueItem.Metadata;
                        if (videoMetadata is null)
                        {
                            var videoMetadataResult = await youtubeMetadataService.GetVideoMetadataAsync(queueItem.Source);
                            if (!videoMetadataResult.IsSuccess)
                                return result = new SubtitlesFetchResult.Failed(
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
                return result = new SubtitlesFetchResult.ActionRequired(
                    $"Subtitles for '{language.Value.DisplayName}' language already fetched, try other languages",
                    null,
                    SubtitlesActionRequired.LanguageSelection,
                    autoSubtitlesOption);

            var subtitlesResult = await youtubeSubtitlesService.FetchAutoSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                subtitlesDirectory,
                language.Value.Iso6391Code);

            if (!subtitlesResult.IsSuccess)
                return result = new SubtitlesFetchResult.ActionRequired(
                    $"Failed to fetch auto subtitles for '{language.Value.DisplayName}', please select another language and try again",
                    subtitlesResult.Error.Message,
                    SubtitlesActionRequired.LanguageSelection,
                    autoSubtitlesOption,
                    true);

            if (subtitlesResult.Value is not null)
            {
                queueItem.Subtitles.SetAutoSubtitles(subtitlesResult.Value);
                return result = new SubtitlesFetchResult.Fetched(
                    new SubtitlesFetchResult.ActionRequired(
                        $"Auto subtitles were fetched for '{language.Value.DisplayName}', you can try other languages",
                        null,
                        SubtitlesActionRequired.LanguageSelection,
                        autoSubtitlesOption));
            }

            return result = new SubtitlesFetchResult.ActionRequired(
                $"Auto subtitles for '{language.Value.DisplayName}' not found, please select another language and try again",
                null,
                SubtitlesActionRequired.LanguageSelection,
                autoSubtitlesOption,
                true);
        }
        catch (OperationCanceledException)
        {
            if (didAutoSubtitlesOptionChange)
            {
                return result = new SubtitlesFetchResult.ActionRequired(
                    "Auto Subtitles Option settings were changed, please click refresh",
                    null,
                    SubtitlesActionRequired.RefreshDueToSettingsChange,
                    null);
            }

            return result = queueItem.Subtitles.LastSeenAutoSubtitlesFetchResult switch
            {
                SubtitlesFetchResult.ActionRequired r
                    when r.CreatedWithOption != userSettings.Current.AutoSubtitlesOption
                        || r.CreatedWithOption is null =>
                    new SubtitlesFetchResult.Canceled(Errors.Youtube.AutoSubtitlesFetchCanceled().Message),
                SubtitlesFetchResult.ActionRequired r => r,
                SubtitlesFetchResult.Fetched r
                    when r.Action is not null => r.Action,
                _ => new SubtitlesFetchResult.Canceled(Errors.Youtube.AutoSubtitlesFetchCanceled().Message)
            };
        }
        finally
        {
            queueItem.Subtitles.FinishAutoSubtitlesLoading(
                result ?? new SubtitlesFetchResult.Failed(
                    "Unexpected subtitles fetch error"));
            tempDirectoryService.DeleteSubdirectory(subtitlesDirectory);
            userSettings.Changed -= onSettingsChanged;
        }
    }
}
