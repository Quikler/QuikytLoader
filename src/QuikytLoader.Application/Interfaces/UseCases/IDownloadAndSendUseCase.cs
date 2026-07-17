using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Interfaces.UseCases;

public interface IDownloadAndSendUseCase
{
    public Task<Result> ExecuteAsync(
        DownloadSource downloadSource,
        string? customTitle,
        IProgress<double> progress,
        CancellationToken ct = default);
}
