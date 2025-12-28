namespace QuikytLoader.Infrastructure.Persistence.DTOs;

/// <summary>
/// Internal DTO for Dapper mapping from database.
/// Must be internal for Dapper.AOT source generator compatibility.
/// </summary>
internal class DownloadRecordDto
{
    public string YouTubeId { get; set; } = string.Empty;
    public string VideoTitle { get; set; } = string.Empty;
    public string DownloadedAt { get; set; } = string.Empty;
}
