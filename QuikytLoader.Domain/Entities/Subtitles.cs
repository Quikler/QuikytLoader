using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Domain.Entities;

public sealed class Subtitles
{
    public Guid QueueItemId { get; }

    public bool AllowManualSubtitlesLoading { get; private set; } = true;
    public bool AllowAutoSubtitlesLoading { get; private set; } = true;

    public bool AreAutoSubtitlesLoaded { get; private set; } = false;

    private IReadOnlyDictionary<string, string>? _manualSubtitles;
    private IReadOnlyDictionary<string, string>? _autoSubtitles;

    internal Subtitles(Guid queueItemId) => QueueItemId = queueItemId;

    public IReadOnlyDictionary<string, string>? Dictionary { get; private set; }

    public SubtitleFetchResult? LastSeenAutoSubtitleFetchResult { get; private set; }

    public bool ExistWithLanguage(string language) =>
        (_manualSubtitles is not null && _manualSubtitles.ContainsKey(language)) ||
        (_autoSubtitles is not null && _autoSubtitles.ContainsKey(language));

    public bool StartManualSubtitlesLoading()
    {
        if (!AllowManualSubtitlesLoading)
            return false;

        AllowManualSubtitlesLoading = false;
        return true;
    }

    public bool StartAutoSubtitlesLoading()
    {
        if (!AllowAutoSubtitlesLoading)
            return false;

        AllowAutoSubtitlesLoading = false;
        return true;
    }

    public void FinishManualSubtitlesLoading(SubtitleFetchResult result)
    {
        AllowManualSubtitlesLoading = result switch
        {
            SubtitleFetchResult.Failed => true,
            SubtitleFetchResult.Canceled => true,
            _ => false
        };
    }

    public void FinishAutoSubtitlesLoading(SubtitleFetchResult result)
    {
        AllowAutoSubtitlesLoading = result switch
        {
            SubtitleFetchResult.Fetched => true,
            SubtitleFetchResult.NotFound => true,
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
        if (_autoSubtitles is null)
            _autoSubtitles = subtitles;
        else
        {
            var initialAutoSubtitles = new Dictionary<string, string>(_autoSubtitles);
            foreach (var kvp in subtitles)
            {
                initialAutoSubtitles[kvp.Key] = kvp.Value;
            }
            _autoSubtitles = initialAutoSubtitles;
        }

        RebuildSubtitles();
        AreAutoSubtitlesLoaded = true;
    }

    private void RebuildSubtitles()
    {
        if (_manualSubtitles is null)
        {
            Dictionary = _autoSubtitles?.ToDictionary(
                kvp => $"{kvp.Key} (auto-generated)",
                kvp => kvp.Value);
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
    public sealed record Fetched(ActionRequired? Action = null) : SubtitleFetchResult;
    public sealed record NotFound(string Message) : SubtitleFetchResult;
    public sealed record Failed(string Message, string? DetailsMessage = null) : SubtitleFetchResult;
    public sealed record Canceled(string Message) : SubtitleFetchResult;
    public sealed record NotAllowed : SubtitleFetchResult;

    public sealed record ActionRequired(
        string Message,
        string? DetailsMessage,
        SubtitleActionRequired SubtitleActionRequired,
        AutoSubtitlesOption? CreatedWithOption,
        bool IsError = false) : SubtitleFetchResult;
}
