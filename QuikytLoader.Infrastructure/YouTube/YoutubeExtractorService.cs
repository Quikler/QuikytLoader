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

    public Result<YouTubeId> GetVideoId(string youtubeUrl)
    {
        if (string.IsNullOrWhiteSpace(youtubeUrl))
            return Errors.YouTube.InvalidUrl(youtubeUrl);

        var match = YoutubeIdRegex().Match(youtubeUrl);
        if (match.Success && match.Groups.Count > 1)
        {
            var idString = match.Groups[1].Value;
            var regexIdResult = YouTubeId.Create(idString);
            if (regexIdResult.IsSuccess)
                return regexIdResult;
        }

        return Errors.YouTube.VideoIdExtractionFailed(youtubeUrl);
    }

    public async Task<Result<string>> GetVideoTitleAsync(string youtubeUrl, CancellationToken cancellationToken = default)
        => await ytDlpService.GetVideoTitleAsync(youtubeUrl, cancellationToken);
}
