using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for yt-dlp process execution service.
/// </summary>
public interface IYtDlpService
{
    Task<Result> DownloadAudioAsync(string youtubeVideoId, string tempDirectory, string? customTitle = null, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    Task<Result<VideoMetadata>> GetVideoMetadataAsync(string youtubeVideoId, CancellationToken cancellationToken = default);

    Task<Result<PlaylistMetadataDto>> GetPlaylistMetadataAsync(string youtubePlaylistId, uint maxItems, CancellationToken cancellationToken = default);
}
