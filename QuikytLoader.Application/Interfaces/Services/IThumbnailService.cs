using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Service for processing thumbnails to meet platform-specific requirements
/// </summary>
public interface IThumbnailService
{
    /// <summary>
    /// Processes thumbnail to meet Telegram requirements: 320x320 max, JPEG format, square crop
    /// </summary>
    /// <param name="thumbnailPath">Path to the thumbnail file to process</param>
    /// <returns>Result indicating success or failure</returns>
    Result ProcessForTelegram(string thumbnailPath);
}