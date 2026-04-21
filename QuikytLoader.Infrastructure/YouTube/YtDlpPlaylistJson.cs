using System.Text.Json.Serialization;

namespace QuikytLoader.Infrastructure.YouTube;

/// <summary>
/// Shape of yt-dlp --flat-playlist --dump-single-json output (minimal fields we need).
/// </summary>
internal sealed class YtDlpPlaylistJson
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("entries")]
    public List<YtDlpPlaylistEntryJson> Entries { get; set; } = [];
}

internal sealed class YtDlpPlaylistEntryJson
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("availability")]
    public string? Availability { get; set; }

    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    /// <summary>
    /// Duration in seconds (flat-playlist returns a number, not a formatted string).
    /// </summary>
    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("thumbnails")]
    public List<YtDlpThumbnailJson>? Thumbnails { get; set; }
}

internal sealed class YtDlpThumbnailJson
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
