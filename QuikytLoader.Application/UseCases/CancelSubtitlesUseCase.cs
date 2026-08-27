using QuikytLoader.Application.Interfaces.Services;

namespace QuikytLoader.Application.UseCases;

public interface ICancelSubtitlesUseCase
{
    void Execute(Guid itemId);
}

public class CancelSubtitlesUseCase(
    IYoutubeSubtitlesService youtubeSubtitlesService)
        : ICancelSubtitlesUseCase
{
    public void Execute(Guid itemId)
        => youtubeSubtitlesService.CancelSubtitlesFetching(itemId);
}
