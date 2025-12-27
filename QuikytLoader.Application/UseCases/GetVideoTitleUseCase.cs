using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Get video title without downloading
/// </summary>
public class GetVideoTitleUseCase(IYoutubeExtractorService extractorService)
{
    public async Task<Result<string>> GetVideoTitleAsync(string youtubeUrl)
    {
        var youtubeUrlResult = YouTubeUrl.Create(youtubeUrl);
        return !youtubeUrlResult.IsSuccess
            ? youtubeUrlResult.Error
            : await extractorService.GetVideoTitleAsync(youtubeUrlResult.Value.Value);
    }
}
