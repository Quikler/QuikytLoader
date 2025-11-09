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
    /// <returns>The full path to the downloaded MP3 file</returns>
    System.Threading.Tasks.Task<string> DownloadAsync(string url, System.IProgress<double>? progress = null);
}
