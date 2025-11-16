namespace QuikytLoader.Models;

/// <summary>
/// Represents the result of a YouTube download operation
/// Contains paths to both the audio file and optional thumbnail
/// </summary>
public class DownloadResult
{
    /// <summary>
    /// Full path to the downloaded MP3 audio file
    /// </summary>
    public required string AudioFilePath { get; init; }

    /// <summary>
    /// Full path to the downloaded thumbnail image (JPEG format)
    /// Null if thumbnail download failed or was not available
    /// </summary>
    public string? ThumbnailPath { get; init; }
}
