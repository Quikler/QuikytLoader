using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Fetch video information without downloading
/// Validates URL before fetching to ensure domain integrity
/// </summary>
public class GetVideoInfoUseCase(IYouTubeDownloadService downloadService)
{
    /// <summary>
    /// Gets the video title from YouTube without downloading the video.
    /// Validates that the URL is a proper YouTube URL before fetching.
    /// Returns a Result containing the title on success, or an Error on failure.
    /// </summary>
    /// <param name="url">YouTube video URL</param>
    /// <returns>Result containing the video title, or error details if fetch fails</returns>
    public async Task<Result<string>> GetVideoTitleAsync(string url)
    {
        // Validate URL format using value object
        var youtubeUrlResult = YouTubeUrl.Create(url);
        if (!youtubeUrlResult.IsSuccess)
            return Result<string>.Failure(Errors.YouTube.InvalidUrl(url));

        return await downloadService.GetVideoTitleAsync(youtubeUrlResult.Value.Value);
    }
}
