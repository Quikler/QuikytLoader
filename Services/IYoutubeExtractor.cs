using System.Threading;
using System.Threading.Tasks;

namespace QuikytLoader.Services;

/// <summary>
/// Service for extracting YouTube video IDs from URLs.
/// </summary>
public interface IYoutubeExtractor
{
    /// <summary>
    /// Extracts the YouTube video ID from a given URL.
    /// </summary>
    /// <param name="url">The YouTube URL to extract the ID from.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The extracted video ID, or null if extraction fails.</returns>
    Task<string?> ExtractIdAsync(string url, CancellationToken cancellationToken = default);
}
