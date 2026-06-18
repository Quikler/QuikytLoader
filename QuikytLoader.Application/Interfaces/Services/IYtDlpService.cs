using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for yt-dlp process execution service.
/// </summary>
public interface IYtDlpService
{
    Task<Result> DownloadAudioAsync(
        DownloadSource downloadSource,
        string tempDirectory,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    Task<Result<VideoMetadata>> GetVideoMetadataAsync(
        DownloadSource downloadSource,
        CancellationToken cancellationToken = default);

    Task<Result<PlaylistMetadataDto>> GetPlaylistMetadataAsync(
        DownloadPlaylistSource downloadPlaylistSource,
        uint maxItems,
        CancellationToken cancellationToken = default);
}
