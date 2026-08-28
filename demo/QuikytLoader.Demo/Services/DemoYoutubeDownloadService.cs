using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.Services;

internal sealed class DemoYoutubeDownloadService
    : IYoutubeDownloadService
{
    public async Task<Result<DownloadResultEntity>> DownloadAudioAsync(
        string downloadDirectory,
        DownloadSource downloadSource,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await SimulateProgress(100, 20, progress, cancellationToken);
        return CreateDownloadResultEntity(
            downloadSource,
            downloadDirectory,
            customTitle);
    }
}
