using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for yt-dlp process execution service.
/// </summary>
public interface IYtDlpService
{
    Task<Result<string>> GetVideoIdAsync(string url, CancellationToken cancellationToken = default);

    Task<Result<string>> GetVideoTitleAsync(string url, CancellationToken cancellationToken = default);

    Task<Result> DownloadAudioAsync(string url, string tempDirectory, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    Task<Result<VideoMetadataDto>> GetVideoMetadataAsync(string url, CancellationToken cancellationToken = default);

    Task<Result<PlaylistMetadataDto>> GetPlaylistMetadataAsync(string url, int maxItems, CancellationToken cancellationToken = default);
}
