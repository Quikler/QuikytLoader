using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing the result of a YouTube download operation
/// Contains paths to temporary files (both audio and thumbnail)
/// These files are stored in temp directory and should be cleaned up after use
/// </summary>
public record DownloadResultEntity(
    YouTubeId YouTubeId,
    string VideoTitle,
    string TempMediaFilePath,
    string? TempThumbnailPath);
