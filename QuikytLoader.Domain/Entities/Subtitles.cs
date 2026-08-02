using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Domain.Entities;

public sealed class Subtitles
{
    private bool _allowManualLoading = true;
    private bool _allowAutoLoading = true;

    private IReadOnlyDictionary<string, string>? _manualSubtitles;
    private IReadOnlyDictionary<string, string>? _autoSubtitles;

    internal Subtitles() { }

    public IReadOnlyDictionary<string, string>? Dictionary { get; private set; }

    public SubtitleFetchResult? LastSeenAutoSubtitleFetchResult { get; private set; }

    public bool StartManualSubtitlesLoading()
    {
        if (!_allowManualLoading)
            return false;

        _allowManualLoading = false;
        return true;
    }

    public bool StartAutoSubtitlesLoading()
    {
        if (!_allowAutoLoading)
            return false;

        _allowAutoLoading = false;
        return true;
    }

    public void FinishManualSubtitlesLoading(SubtitleFetchResult result)
    {
        _allowManualLoading = result switch
        {
            SubtitleFetchResult.Failed => true,
            SubtitleFetchResult.Canceled => true,
            _ => false
        };
    }

    public void FinishAutoSubtitlesLoading(SubtitleFetchResult result)
    {
        _allowAutoLoading = result switch
        {
            SubtitleFetchResult.Failed => true,
            SubtitleFetchResult.Canceled => true,
            SubtitleFetchResult.ActionRequired => true,
            _ => false
        };

        LastSeenAutoSubtitleFetchResult = result;
    }

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
            Dictionary = null;
            return;
        }

        if (_manualSubtitles is null)
        {
            Dictionary = _autoSubtitles;
            return;
        }

        if (_autoSubtitles is null)
        {
            Dictionary = _manualSubtitles;
            return;
        }

        var mergedDictionary = new Dictionary<string, string>(_manualSubtitles);

        foreach (var kvp in _autoSubtitles)
        {
            if (!mergedDictionary.ContainsKey(kvp.Key))
                mergedDictionary[$"{kvp.Key} (auto-generated)"] = kvp.Value;
        }

        Dictionary = mergedDictionary;
    }
}

public abstract record SubtitleFetchResult
{
    public sealed record Fetched : SubtitleFetchResult;
    public sealed record NotFound(string Message, bool AllowRetry) : SubtitleFetchResult;
    public sealed record Failed(string Message, bool AllowRetry, string? DetailsMessage = null) : SubtitleFetchResult;
    public sealed record Canceled(string Message, bool AllowRetry) : SubtitleFetchResult;
    public sealed record NotAllowed : SubtitleFetchResult;

    public sealed record ActionRequired(string Message, string? DetailsMessage, SubtitleActionRequired SubtitleActionRequired, bool IsError = false) : SubtitleFetchResult;
}
