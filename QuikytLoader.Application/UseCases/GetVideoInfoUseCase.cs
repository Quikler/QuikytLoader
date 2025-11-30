using QuikytLoader.Application.Interfaces.Services;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Fetch video information without downloading
/// </summary>
public class GetVideoInfoUseCase
{
    private readonly IYouTubeDownloadService _downloadService;

    public GetVideoInfoUseCase(IYouTubeDownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    /// <summary>
    /// Gets the video title from YouTube without downloading the video
    /// </summary>
    /// <param name="url">YouTube video URL</param>
    /// <returns>The video title</returns>
    public async Task<string> GetVideoTitleAsync(string url)
    {
        return await _downloadService.GetVideoTitleAsync(url);
    }
}
