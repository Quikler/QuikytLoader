using System.Text.Json.Serialization;

namespace QuikytLoader.Infrastructure.Youtube;

internal sealed class YtDlpVideoJson
{
    [JsonPropertyName("title")]
    [JsonRequired]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("duration")]
    [JsonRequired]
    public double Duration { get; set; }
}

/// <summary>
/// Shape of yt-dlp --flat-playlist --dump-single-json output (minimal fields we need).
/// </summary>
internal sealed class YtDlpPlaylistJson
{
    [JsonPropertyName("title")]
    [JsonRequired]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("entries")]
    [JsonRequired]
    public List<YtDlpPlaylistEntryJson> Entries { get; set; } = [];
}

internal sealed class YtDlpPlaylistEntryJson
{
    [JsonPropertyName("id")]
    [JsonRequired]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    [JsonRequired]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    [JsonRequired]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("duration")]
    [JsonRequired]
    public double Duration { get; set; }
}
