using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Service interface for downloading videos from YouTube
/// </summary>
public interface IYoutubeDownloadService
{
    /// <summary>
    /// Downloads a video from YouTube and converts it to MP3 format.
    /// </summary>
    /// <param name="customTitle">Optional custom filename (without extension)</param>
    Task<Result<DownloadResultDto>> DownloadAudioAsync(string url, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
