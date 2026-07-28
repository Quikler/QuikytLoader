using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Domain.Entities;

public sealed class QueueItem
{
    private bool _allowManualSubtitlesLoading = true;
    private bool _allowAutoSubtitlesLoading = true;

    private IReadOnlyDictionary<string, string>? _manualSubtitles;
    private IReadOnlyDictionary<string, string>? _autoSubtitles;

    public Guid Id { get; } = Guid.NewGuid();

    public string? GroupId { get; init; }

    public required DownloadSource Source { get; init; }

    public required VideoMetadata? Metadata { get; set; }

    public string? CustomTitle { get; set; }

    public double Progress { get; set; }

    public DownloadStatus Status { get; set; }

    public Error? Error { get; set; }

    public IReadOnlyDictionary<string, string>? Subtitles { get; private set; }

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

    public bool StartManualSubtitlesLoading()
    {
        if (!_allowManualSubtitlesLoading)
            return false;

        _allowManualSubtitlesLoading = false;
        return true;
    }

    public bool StartAutoSubtitlesLoading()
    {
        if (!_allowAutoSubtitlesLoading)
            return false;

        _allowAutoSubtitlesLoading = false;
        return true;
    }

    public void FinishManualSubtitlesLoading(SubtitleFetchResult result) =>
        _allowManualSubtitlesLoading = result switch
        {
            SubtitleFetchResult.Fetched => false,
            SubtitleFetchResult.NotFound => false,
            SubtitleFetchResult.Failed => true,
            SubtitleFetchResult.Canceled => true,
            SubtitleFetchResult.NotAllowed => false,
            _ => false
        };

    public void FinishAutoSubtitlesLoading(SubtitleFetchResult result) =>
        _allowAutoSubtitlesLoading = result switch
        {
            SubtitleFetchResult.Fetched => false,
            SubtitleFetchResult.NotFound => false,
            SubtitleFetchResult.Failed => true,
            SubtitleFetchResult.Canceled => true,
            SubtitleFetchResult.RequiresLanguageSelection => true,
            SubtitleFetchResult.LanguageMayBeWrong => true,
            _ => false
        };

    public void SetManualSubtitles(IReadOnlyDictionary<string, string> subtitles)
    {
        _manualSubtitles = subtitles;
        RebuildSubtitles();
    }

    public void SetAutoSubtitles(IReadOnlyDictionary<string, string> subtitles)
    {
        _autoSubtitles = subtitles;
        RebuildSubtitles();
    }

    private void RebuildSubtitles()
    {
        if (_manualSubtitles is null && _autoSubtitles is null)
        {
            Subtitles = null;
            return;
        }

        if (_manualSubtitles is null)
        {
            Subtitles = _autoSubtitles;
            return;
        }

        if (_autoSubtitles is null)
        {
            Subtitles = _manualSubtitles;
            return;
        }

        var dict = new Dictionary<string, string>(_manualSubtitles);

        foreach (var kvp in _autoSubtitles)
        {
            if (!dict.ContainsKey(kvp.Key))
                dict[$"{kvp.Key} (auto-generated)"] = kvp.Value;
        }

        Subtitles = dict;
    }
}

public record DownloadSource(string YoutubeVideoUrl, string YoutubeVideoId);

public record DownloadPlaylistSource(string YoutubePlaylistUrl, string YoutubePlaylistId);

public record VideoMetadata(
    string Title,
    string? Channel,
    string? Description,
    TimeSpan DurationInSeconds);

public abstract record SubtitleFetchResult
{
    public sealed record Fetched : SubtitleFetchResult;
    public sealed record NotFound(string Message, bool AllowRetry) : SubtitleFetchResult;
    public sealed record Failed(string Message, bool AllowRetry, string? Details = null) : SubtitleFetchResult;
    public sealed record Canceled(string Message, bool AllowRetry) : SubtitleFetchResult;
    public sealed record NotAllowed : SubtitleFetchResult;
    public sealed record RequiresLanguageSelection(string Message) : SubtitleFetchResult;
    public sealed record LanguageMayBeWrong(string Message, string Details) : SubtitleFetchResult;
}
