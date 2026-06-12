using System.Text.RegularExpressions;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Infrastructure.YouTube;

/// <summary>
/// Service for extracting YouTube info.
/// Uses regex for fast extraction, with yt-dlp fallback for edge cases.
/// </summary>
internal partial class YoutubeExtractorService(IYtDlpService ytDlpService) : IYoutubeExtractorService
{
    // Regex patterns for common YouTube URL formats
    // Matches: youtube.com/watch?v=ID, youtu.be/ID, youtube.com/embed/ID, etc.
    [GeneratedRegex(@"(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YoutubeIdRegex();

    [GeneratedRegex(@"(?:[?&]list=)([a-zA-Z0-9_-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex YoutubePlaylistRegex();

    public Result<string> GetPlaylistId(string youtubePlaylistUrl)
    {
        if (string.IsNullOrWhiteSpace(youtubePlaylistUrl))
            return Errors.YouTube.InvalidUrl(youtubePlaylistUrl);

        var match = YoutubePlaylistRegex().Match(youtubePlaylistUrl);
        return match.Success
            ? match.Groups[1].Value
            : Errors.YouTube.InvalidUrl(youtubePlaylistUrl);
    }

    public Result<YouTubeId> GetVideoId(string youtubeUrl)
    {
        if (string.IsNullOrWhiteSpace(youtubeUrl))
            return Errors.YouTube.InvalidUrl(youtubeUrl);

        var match = YoutubeIdRegex().Match(youtubeUrl);
        return match.Success && match.Groups.Count > 1 && YouTubeId.Create(match.Groups[1].Value) is { IsSuccess: true } regexIdResult
            ? regexIdResult
            : Errors.YouTube.InvalidUrl(youtubeUrl);
    }

    public async Task<Result<string>> GetVideoTitleAsync(string youtubeUrl, CancellationToken cancellationToken = default)
        => await ytDlpService.GetVideoTitleAsync(youtubeUrl, cancellationToken);
}
