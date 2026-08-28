using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Domain.Entities;

public sealed class QueueItem
{
    public Guid Id { get; }

    public Subtitles Subtitles { get; }

    public DownloadSource Source { get; }

    public required VideoMetadata? Metadata { get; set; }

    public string? CustomTitle { get; set; }

    public double Progress { get; set; }

    public DownloadStatus Status { get; set; }

    public Error? Error { get; set; }

    public QueueItem(DownloadSource downloadSource)
    {
        Id = Guid.NewGuid();
        Subtitles = new(Id);

        Source = downloadSource;
    }

    public bool CanStartDownload =>
        Status is DownloadStatus.Queued
            or DownloadStatus.Failed
            or DownloadStatus.Cancelled
            or DownloadStatus.Editing;

    public bool CanCancel =>
        Status is DownloadStatus.Downloading
            or DownloadStatus.Pending;

    public bool CanEdit =>
        Status is DownloadStatus.Editing
            or DownloadStatus.Cancelled
            or DownloadStatus.Failed;
}

public record DownloadSource(string YoutubeVideoUrl, string YoutubeVideoId);

public record DownloadPlaylistSource(string YoutubePlaylistUrl, string YoutubePlaylistId);

public record VideoMetadata(
    string Title,
    string? Channel,
    string? Description,
    TimeSpan DurationInSeconds);
