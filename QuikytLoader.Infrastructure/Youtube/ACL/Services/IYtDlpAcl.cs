using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Infrastructure.Youtube.ACL.RawModels;

namespace QuikytLoader.Infrastructure.Youtube.ACL.Services;

internal interface IYtDlpAcl
{
    Task<Result<YtDlpVideoRaw>> GetVideoAsync(DownloadSource downloadSource, CancellationToken ct);

    Task<Result<YtDlpPlaylistRaw>> GetPlaylistAsync(DownloadPlaylistSource downloadPlaylistSource, int maxItems, CancellationToken ct);

    Task<Result> DownloadAudioAsync(
        DownloadSource downloadSource,
        string downloadDirectory,
        string? fileName,
        Action<string>? onOutputLine,
        Action<string>? onErrorLine,
        CancellationToken ct);
}
