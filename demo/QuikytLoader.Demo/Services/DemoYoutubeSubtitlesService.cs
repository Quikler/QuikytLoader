using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Demo.Services;

internal sealed class DemoYoutubeSubtitlesService
    : IYoutubeSubtitlesService
{
    private readonly Dictionary<Guid, CancellationTokenSource> _cancellationTokens = [];

    public async Task<Result<IReadOnlyDictionary<string, string>?>> FetchManualSubtitlesAsync(
        Guid itemId,
        DownloadSource downloadSource,
        string tempDirectory)
    {
        var cts = _cancellationTokens[itemId] = new CancellationTokenSource();
        var ct = cts.Token;

        try
        {
            // Simulate subtitles fetching
            await Task.Delay(Random.Shared.Next(1000, 2000), ct);

            var subtitles = CreateSubtitles();

            return Result<IReadOnlyDictionary<string, string>?>.Success(
                subtitles.Count == 0 ? null : subtitles);
        }
        finally
        {
            cts.Dispose();
            _cancellationTokens.Remove(itemId);
        }
    }

    public async Task<Result<IReadOnlyDictionary<string, string>?>> FetchAutoSubtitlesAsync(
        Guid itemId,
        DownloadSource downloadSource,
        string tempDirectory,
        string? language)
    {
        var cts = _cancellationTokens[itemId] = new CancellationTokenSource();
        var ct = cts.Token;

        try
        {
            // Simulate subtitles fetching
            await Task.Delay(Random.Shared.Next(1000, 2000), ct);

            var subtitles = CreateSubtitles();

            if (!string.IsNullOrWhiteSpace(language))
            {
                subtitles = subtitles
                    .Where(x => x.Key == language)
                    .ToDictionary();
            }

            return Result<IReadOnlyDictionary<string, string>?>.Success(
                subtitles.Count == 0 ? null : subtitles);
        }
        finally
        {
            cts.Dispose();
            _cancellationTokens.Remove(itemId);
        }
    }

    public void CancelSubtitlesFetching(Guid itemId)
        => _cancellationTokens[itemId].Cancel();
}
