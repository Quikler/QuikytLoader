using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Fetch playlist metadata (title + up to maxItems entries) without downloading.
/// </summary>
public class GetPlaylistMetadataUseCase(IYtDlpService ytDlpService)
{
    public async Task<Result<PlaylistMetadataDto>> GetMetadataAsync(
        string playlistUrl,
        int maxItems = 15,
        CancellationToken cancellationToken = default)
    {
        var urlResult = YouTubePlaylistUrl.Create(playlistUrl);
        return !urlResult.IsSuccess
            ? urlResult.Error
            : await ytDlpService.GetPlaylistMetadataAsync(urlResult.Value.Value, maxItems, cancellationToken);
    }
}
