using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

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
    Task<Result<DownloadResultEntity>> DownloadAudioAsync(string url, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
