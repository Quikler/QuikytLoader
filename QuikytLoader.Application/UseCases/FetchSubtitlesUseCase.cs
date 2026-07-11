using QuikytLoader.Application.Interfaces.Services;

namespace QuikytLoader.Application.UseCases;

public class FetchSubtitlesUseCase(IYoutubeSubtitlesService youtubeSubtitlesService)
{
    public void Execute(Guid itemId) => _ = youtubeSubtitlesService.FetchSubtitlesAsync(itemId);
}
