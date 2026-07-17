using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Application.UseCases;

public abstract record FetchSubtitlesResult
{
    public sealed record Failed() : FetchSubtitlesResult;
    public sealed record SubtitlesNotFound() : FetchSubtitlesResult;
    public sealed record SubtitlesFetchCanceled() : FetchSubtitlesResult;
    public sealed record ManuallySelectedLanguageMightBeWrong(string Message, Error InitialError) : FetchSubtitlesResult;
    public sealed record SubtitlesFetched() : FetchSubtitlesResult;
    public sealed record SubtitlesNotAllowedToBeLoaded() : FetchSubtitlesResult;
    public sealed record ManualLanguageSelectionRequired(string Message) : FetchSubtitlesResult;
}

public class FetchSubtitlesUseCase(IYoutubeSubtitlesService youtubeSubtitlesService, IDownloadQueue queue, IUserSettings userSettings, ITempDirectoryService tempDirectoryService)
{
    public async Task<FetchSubtitlesResult> ExecuteAsync(Guid itemId, string? manuallySelectedLanguageForAutoSubtitles = null)
    {
        var queueItem = queue.GetItem(itemId);
        // Subtitles have already been loaded or are currently loading or are not allowed to be loaded
        if (queueItem.Subtitles is not null || queueItem.AreSubtitlesLoading || !queueItem.AllowSubtitlesLoading)
            return new FetchSubtitlesResult.SubtitlesNotAllowedToBeLoaded();

        queueItem.AreSubtitlesLoading = true;
        queueItem.SubtitlesError = null;
        queue.UpdateItem(queueItem.Id);

        var subtitlesDirectory =
            tempDirectoryService.CreateSubdirectory(queueItem.Source.YoutubeVideoId, "subtitles");

        try
        {
            Result<IReadOnlyDictionary<string, string>?> subtitlesResult;
            if (manuallySelectedLanguageForAutoSubtitles is null)
            {
                subtitlesResult = await youtubeSubtitlesService.FetchManualSubtitlesAsync(queueItem.Id, queueItem.Source, subtitlesDirectory);
                if (!subtitlesResult.IsSuccess)
                {
                    queueItem.SubtitlesError = subtitlesResult.Error;
                    return new FetchSubtitlesResult.Failed();
                }

                if (subtitlesResult.Value is not null)
                {
                    queueItem.Subtitles = subtitlesResult.Value;
                    queueItem.AllowSubtitlesLoading = false;
                    return new FetchSubtitlesResult.SubtitlesFetched();
                }
            }

            var autoSubtitlesOption = userSettings.Load().AutoSubtitlesOption;
            switch (autoSubtitlesOption)
            {
                case AutoSubtitlesOption.ManualLanguageSelection when manuallySelectedLanguageForAutoSubtitles is null:
                    return new FetchSubtitlesResult.ManualLanguageSelectionRequired("Please select video language");
                case AutoSubtitlesOption.AutoLanguageDetection:
                    break;
                case AutoSubtitlesOption.FallbackToEnglishLanguage:
                    manuallySelectedLanguageForAutoSubtitles = "en";
                    break;
            }

            subtitlesResult = await youtubeSubtitlesService.FetchAutoSubtitlesAsync(
                queueItem.Id,
                queueItem.Source,
                queueItem.Metadata,
                subtitlesDirectory,
                manuallySelectedLanguageForAutoSubtitles);
            if (!subtitlesResult.IsSuccess)
            {
                if (autoSubtitlesOption == AutoSubtitlesOption.ManualLanguageSelection)
                    return new FetchSubtitlesResult.ManuallySelectedLanguageMightBeWrong("Failed to fetch auto subtitles - please verify your language and try again", subtitlesResult.Error);

                queueItem.SubtitlesError = subtitlesResult.Error;
                return new FetchSubtitlesResult.Failed();
            }

            if (subtitlesResult.Value is not null)
            {
                queueItem.Subtitles = subtitlesResult.Value;
                queueItem.AllowSubtitlesLoading = false;
                return new FetchSubtitlesResult.SubtitlesFetched();
            }

            queueItem.AllowSubtitlesLoading = false;
            queueItem.SubtitlesError = Errors.Youtube.SubtitlesNotFound();
            return new FetchSubtitlesResult.SubtitlesNotFound();
        }
        catch (OperationCanceledException)
        {
            queueItem.SubtitlesError = Errors.Youtube.SubtitlesFetchCanceled();
            return new FetchSubtitlesResult.SubtitlesFetchCanceled();
        }
        finally
        {
            queueItem.AreSubtitlesLoading = false;
            queue.UpdateItem(queueItem.Id);
            tempDirectoryService.DeleteSubdirectory(subtitlesDirectory);
        }
    }
}
