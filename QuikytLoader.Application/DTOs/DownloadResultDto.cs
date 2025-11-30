namespace QuikytLoader.Application.DTOs;

/// <summary>
/// Data transfer object representing the result of a YouTube download operation
/// Contains paths to temporary files (both audio and thumbnail)
/// These files are stored in temp directory and should be cleaned up after use
/// </summary>
public class DownloadResultDto
{
    /// <summary>
    /// YouTube video ID (11 characters)
    /// </summary>
    public required string YouTubeId { get; init; }

    /// <summary>
    /// Full path to the temporary MP3 audio file
    /// File is in temp directory and should be deleted after sending to Telegram
    /// </summary>
    public required string TempMediaFilePath { get; init; }

    /// <summary>
    /// Full path to the temporary thumbnail image (JPEG format)
    /// Null if thumbnail download failed or was not available
    /// File is in temp directory and should be deleted after sending to Telegram
    /// </summary>
    public string? TempThumbnailPath { get; init; }
}
