namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing the result of a Youtube download operation
/// Contains paths to temporary files (both audio and thumbnail)
/// These files are stored in temp directory and should be cleaned up after use
/// </summary>
public record DownloadResultEntity(
    string YoutubeVideoId,
    string VideoTitle,
    string TempMp3FilePath,
    string TempThumbnailFilePath);
