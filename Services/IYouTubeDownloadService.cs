using QuikytLoader.Models;

namespace QuikytLoader.Services;

/// <summary>
/// Service interface for downloading videos from YouTube
/// </summary>
public interface IYouTubeDownloadService
{
    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format
    /// </summary>
    /// <param name="url">The YouTube video URL</param>
    /// <param name="progress">Optional progress reporter (0-100)</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the download</param>
    /// <returns>Download result containing paths to the MP3 file and thumbnail</returns>
    System.Threading.Tasks.Task<DownloadResult> DownloadAsync(string url, System.IProgress<double>? progress = null, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the video title from YouTube without downloading
    /// </summary>
    /// <param name="url">The YouTube video URL</param>
    /// <returns>The video title</returns>
    System.Threading.Tasks.Task<string> GetVideoTitleAsync(string url);

    /// <summary>
    /// Downloads a video from YouTube with a custom filename and converts it to MP3 format
    /// </summary>
    /// <param name="url">The YouTube video URL</param>
    /// <param name="customTitle">Custom filename (without extension)</param>
    /// <param name="progress">Optional progress reporter (0-100)</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the download</param>
    /// <returns>Download result containing paths to the MP3 file and thumbnail</returns>
    System.Threading.Tasks.Task<DownloadResult> DownloadAsync(string url, string customTitle, System.IProgress<double>? progress = null, System.Threading.CancellationToken cancellationToken = default);
}
