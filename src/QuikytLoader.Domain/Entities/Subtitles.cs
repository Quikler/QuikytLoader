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

    public SubtitlesFetchResult? LastSeenAutoSubtitlesFetchResult { get; private set; }

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

    public void FinishManualSubtitlesLoading(SubtitlesFetchResult result)
    {
        AllowManualSubtitlesLoading = result switch
        {
            SubtitlesFetchResult.Failed => true,
            SubtitlesFetchResult.Canceled => true,
            _ => false
        };
    }

    public void FinishAutoSubtitlesLoading(SubtitlesFetchResult result)
    {
        AllowAutoSubtitlesLoading = result switch
        {
            SubtitlesFetchResult.Fetched => true,
            SubtitlesFetchResult.NotFound => true,
            SubtitlesFetchResult.Failed => true,
            SubtitlesFetchResult.Canceled => true,
            SubtitlesFetchResult.ActionRequired => true,
            _ => false
        };

        LastSeenAutoSubtitlesFetchResult = result;
    }

    public void SetManualSubtitles(IReadOnlyDictionary<string, string> subtitles)
    {
        subtitles = NormalizeLanguageKeys(subtitles);

        _manualSubtitles = subtitles;
        RebuildSubtitles();
    }

    public void SetAutoSubtitles(IReadOnlyDictionary<string, string> subtitles)
    {
        subtitles = NormalizeLanguageKeys(subtitles);

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

    // Normalizes language keys, so we treat "de-DE" as "de"
    private IReadOnlyDictionary<string, string> NormalizeLanguageKeys(
        IReadOnlyDictionary<string, string> rawContent)
            => rawContent
                .GroupBy(kvp => kvp.Key.Split('-')[0].ToLowerInvariant())
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Value,
                    StringComparer.OrdinalIgnoreCase);

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

public abstract record SubtitlesFetchResult
{
    public sealed record Fetched(ActionRequired? Action = null) : SubtitlesFetchResult;
    public sealed record NotFound(string Message) : SubtitlesFetchResult;
    public sealed record Failed(string Message, string? DetailsMessage = null) : SubtitlesFetchResult;
    public sealed record Canceled(string Message) : SubtitlesFetchResult;
    public sealed record NotAllowed : SubtitlesFetchResult;

    public sealed record ActionRequired(
        string Message,
        string? DetailsMessage,
        SubtitlesActionRequired SubtitlesActionRequired,
        AutoSubtitlesOption? CreatedWithOption,
        bool IsError = false) : SubtitlesFetchResult;
}
