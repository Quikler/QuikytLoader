using QuikytLoader.Application.Interfaces.Services;

namespace QuikytLoader.Application.UseCases;

public class CancelSubtitlesUseCase(IYoutubeSubtitlesService youtubeSubtitlesService)
{
    public void Execute(Guid itemId) => youtubeSubtitlesService.CancelSubtitlesFetching(itemId);
}
