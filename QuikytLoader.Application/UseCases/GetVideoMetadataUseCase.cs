using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Get video metadata (title, channel, duration, thumbnail) without downloading
/// </summary>
public class GetVideoMetadataUseCase(IYtDlpService ytDlpService)
{
    public async Task<Result<VideoMetadata>> GetMetadataAsync(string youtubeUrl, CancellationToken cancellationToken = default)
    {
        var youtubeUrlResult = YouTubeUrl.Create(youtubeUrl);
        return !youtubeUrlResult.IsSuccess
            ? youtubeUrlResult.Error
            : await ytDlpService.GetVideoMetadataAsync(youtubeUrlResult.Value.Value, cancellationToken);
    }
}
