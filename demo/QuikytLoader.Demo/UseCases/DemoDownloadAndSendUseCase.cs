using QuikytLoader.Application.Interfaces.UseCases;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.UseCases;

internal sealed class DemoDownloadAndSendUseCase
    : IDownloadAndSendUseCase
{
    public async Task<Result> ExecuteAsync(
        DownloadSource downloadSource,
        string? customTitle,
        IProgress<double> progress,
        CancellationToken ct = default)
    {
        for (var i = 0; i <= 100; i++)
        {
            ct.ThrowIfCancellationRequested();

            progress.Report(i);

            await Task.Delay(
                TimeSpan.FromMilliseconds(20),
                ct);
        }

        return Result.Success();
    }
}
