using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.Services;

public interface IYoutubeDownloadService
{
    /// <summary>
    /// Downloads a video from Youtube and converts it to MP3 format.
    /// </summary>
    /// <param name="metadataTitle">Optional custom title for audio file metadata</param>
    Task<Result<DownloadResultEntity>> DownloadAudioAsync(
        string downloadDirectory,
        DownloadSource downloadSource,
        string? metadataTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}
