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
        Language? manuallySelectedLanguage = null)
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

            var autoSubtitlesOption = userSettings.Current.AutoSubtitlesOption;
            switch (autoSubtitlesOption)
            {
                case AutoSubtitlesOption.ManualLanguageSelection
                    when manuallySelectedLanguage is null:
                    return result = new SubtitleFetchResult.ActionRequired(
                        "Please select video language",
                        null,
                        SubtitleActionRequired.LanguageSelection,
                        autoSubtitlesOption);

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
                    manuallySelectedLanguage =
                        languageIdentifier.Identify($"{videoMetadata.Title}\n{videoMetadata.Description}");
                    break;

                case AutoSubtitlesOption.FallbackToEnglishLanguage:
                    manuallySelectedLanguage = Language.English;
                    break;
            }

            if (manuallySelectedLanguage is null)
                throw new UnreachableException();

            var subtitlesResult = await youtubeSubtitlesService.FetchAutoSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                subtitlesDirectory,
                manuallySelectedLanguage.Value.Iso6391Code);

            if (!subtitlesResult.IsSuccess)
            {
                var (message, action) = autoSubtitlesOption switch
                {
                    AutoSubtitlesOption.ManualLanguageSelection =>
                        ("Failed to fetch auto subtitles - please verify your language and try again",
                        SubtitleActionRequired.LanguageSelection),
                    AutoSubtitlesOption.AutoLanguageDetection =>
                        ($"Failed to fetch auto subtitles (language detected - '{manuallySelectedLanguage.Value.DisplayName}'), please change Auto Subtitles option in settings",
                        SubtitleActionRequired.ChangeAutoSubtitlesOption),
                    AutoSubtitlesOption.FallbackToEnglishLanguage =>
                        ($"Failed to fetch auto subtitles with 'English' language, please change Auto Subtitles option in settings",
                        SubtitleActionRequired.ChangeAutoSubtitlesOption),
                    _ => throw new UnreachableException()
                };

                return result = new SubtitleFetchResult.ActionRequired(
                    message,
                    subtitlesResult.Error.Message,
                    action,
                    autoSubtitlesOption,
                    true);
            }

            if (subtitlesResult.Value is not null)
            {
                queueItem.Subtitles.SetAutoSubtitles(subtitlesResult.Value);
                return result = new SubtitleFetchResult.Fetched();
            }

            // Not found message when no subtitles were found
            if (queueItem.Subtitles.Dictionary is null)
                return result = new SubtitleFetchResult.NotFound(
                    Errors.Youtube.SubtitlesNotFound().Message);

            // Not found message when only no auto subtitles were found
            return result = new SubtitleFetchResult.NotFound(
                Errors.Youtube.AutoSubtitlesNotFound().Message);
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
