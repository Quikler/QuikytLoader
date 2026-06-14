using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Domain.Entities;

public sealed class QueueItem
{
    public Guid Id { get; } = Guid.NewGuid();

    public string? GroupId { get; init; }

    public required DownloadSource Source { get; init; }

    public required VideoMetadata? Metadata { get; set; }

    public string? CustomTitle { get; set; }

    public double Progress { get; set; }

    public DownloadStatus Status { get; set; }

    public Error? Error { get; set; }

    public bool CanStartDownload =>
        Status is DownloadStatus.Queued
            or DownloadStatus.Failed
            or DownloadStatus.Cancelled
            or DownloadStatus.Editing;
}

// Note: SourceId = Youtube VideoId in our app
// SourceId is just generic name not tied specifically to Youtube
public record DownloadSource(string Url, string SourceId);

public record VideoMetadata(
    string VideoId,
    string Title,
    string Channel,
    string Duration,
    string ThumbnailUrl,
    bool IsAvailable,
    string UnavailableReason);
