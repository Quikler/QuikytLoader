using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for yt-dlp process execution service.
/// </summary>
public interface IYtDlpService
{
    bool IsPlaylist(string url);

    bool IsSingleVideo(string url);

    Task<Result<string>> GetVideoTitleAsync(string url, CancellationToken cancellationToken = default);

    Task<Result> DownloadAudioAsync(string url, string tempDirectory, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    Task<Result<VideoMetadata>> GetVideoMetadataAsync(string url, CancellationToken cancellationToken = default);

    Task<Result<PlaylistMetadataDto>> GetPlaylistMetadataAsync(string url, int maxItems, CancellationToken cancellationToken = default);
}
