using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Fetch video information without downloading
/// Validates URL before fetching to ensure domain integrity
/// </summary>
public class GetVideoInfoUseCase(IYouTubeDownloadService downloadService)
{
    /// <summary>
    /// Gets the video title from YouTube without downloading the video
    /// Validates that the URL is a proper YouTube URL before fetching
    /// </summary>
    /// <param name="url">YouTube video URL</param>
    /// <returns>The video title</returns>
    /// <exception cref="ArgumentException">Thrown if URL is not a valid YouTube URL</exception>
    public Task<string> GetVideoTitleAsync(string url) => downloadService.GetVideoTitleAsync(new YouTubeUrl(url).Value);
}
