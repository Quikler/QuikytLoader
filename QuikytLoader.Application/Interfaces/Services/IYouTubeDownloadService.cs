using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Service interface for downloading videos from YouTube
/// </summary>
public interface IYouTubeDownloadService
{
    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format.
    /// Returns a Result containing the download result on success, or an Error on failure.
    /// </summary>
    /// <param name="url">The YouTube video URL</param>
    /// <param name="progress">Optional progress reporter (0-100)</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the download</param>
    /// <returns>Result containing download result with paths to the MP3 file and thumbnail, or error details</returns>
    Task<Result<DownloadResultDto>> DownloadAsync(string url, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the video title from YouTube without downloading.
    /// Returns a Result containing the video title on success, or an Error on failure.
    /// </summary>
    /// <param name="url">The YouTube video URL</param>
    /// <returns>Result containing the video title, or error details if fetch fails</returns>
    Task<Result<string>> GetVideoTitleAsync(string url);

    /// <summary>
    /// Downloads a video from YouTube with a custom filename and converts it to MP3 format.
    /// Returns a Result containing the download result on success, or an Error on failure.
    /// </summary>
    /// <param name="url">The YouTube video URL</param>
    /// <param name="customTitle">Custom filename (without extension)</param>
    /// <param name="progress">Optional progress reporter (0-100)</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the download</param>
    /// <returns>Result containing download result with paths to the MP3 file and thumbnail, or error details</returns>
    Task<Result<DownloadResultDto>> DownloadAsync(string url, string customTitle, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
