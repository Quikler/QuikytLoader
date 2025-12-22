using System.Text.RegularExpressions;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Infrastructure.YouTube;

/// <summary>
/// Service for extracting YouTube video IDs from URLs.
/// Uses regex for fast extraction, with yt-dlp fallback for edge cases.
/// </summary>
internal partial class YoutubeExtractorService(IYtDlpService ytDlpService) : IYoutubeExtractorService
{
    // Regex patterns for common YouTube URL formats
    // Matches: youtube.com/watch?v=ID, youtu.be/ID, youtube.com/embed/ID, etc.
    [GeneratedRegex(@"(?:youtube\.com\/(?:watch\?v=|embed\/|v\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase)]
    private static partial Regex YoutubeIdRegex();

    public async Task<Result<YouTubeId>> GetVideoIdAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Errors.YouTube.InvalidUrl(url);

        var match = YoutubeIdRegex().Match(url);
        if (match.Success && match.Groups.Count > 1)
        {
            var idString = match.Groups[1].Value;
            var regexIdResult = YouTubeId.Create(idString);
            if (regexIdResult.IsSuccess)
                return regexIdResult;
        }

        var videoIdResult = await ytDlpService.GetVideoIdAsync(url, cancellationToken);
        return videoIdResult.IsSuccess
            ? YouTubeId.Create(videoIdResult.Value)
            : Result<YouTubeId>.Failure(videoIdResult.Error);
    }

    public async Task<Result<string>> GetVideoTitleAsync(string url, CancellationToken cancellationToken = default)
    {
        return await ytDlpService.GetVideoTitleAsync(url, cancellationToken);
    }
}
