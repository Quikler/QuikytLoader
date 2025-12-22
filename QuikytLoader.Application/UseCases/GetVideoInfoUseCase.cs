using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Fetch video information without downloading
/// </summary>
public class GetVideoInfoUseCase(IYoutubeExtractorService extractorService)
{
    /// <summary>
    /// Gets the video title from YouTube.
    /// </summary>
    public async Task<Result<string>> GetVideoTitleAsync(string youtubeUrl)
    {
        // Validate URL format using value object
        var youtubeUrlResult = YouTubeUrl.Create(youtubeUrl);
        if (!youtubeUrlResult.IsSuccess)
            return Result<string>.Failure(Errors.YouTube.InvalidUrl(youtubeUrl));

        return await extractorService.GetVideoTitleAsync(youtubeUrlResult.Value.Value);
    }
}
