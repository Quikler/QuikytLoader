using QuikytLoader.Domain.Common;

namespace QuikytLoader.Infrastructure.Youtube.YtDlp;

internal interface IYtDlpProcessClient
{
    Task<Result<string>> RunCaptureAsync(
        IReadOnlyList<string> args,
        CancellationToken ct = default);

    Task<Result> RunStreamingAsync(
        IReadOnlyList<string> args,
        Action<string>? onOutputLine = null,
        CancellationToken ct = default);
}
