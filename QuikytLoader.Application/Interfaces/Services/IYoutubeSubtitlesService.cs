using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Services;

public interface IYoutubeSubtitlesService
{
    Task<Result<IReadOnlyDictionary<string, string>?>> FetchManualSubtitlesAsync(
        Guid itemId,
        DownloadSource downloadSource,
        string tempDirectory);

    Task<Result<IReadOnlyDictionary<string, string>?>> FetchAutoSubtitlesAsync(
        Guid itemId,
        DownloadSource downloadSource,
        string tempDirectory,
        string language);

    void CancelSubtitlesFetching(Guid itemId);
}
