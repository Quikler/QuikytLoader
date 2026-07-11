namespace QuikytLoader.Application.Interfaces.Services;

public interface IYoutubeSubtitlesService
{
    Task FetchSubtitlesAsync(Guid itemId);

    void CancelSubtitles(Guid itemId);
}
