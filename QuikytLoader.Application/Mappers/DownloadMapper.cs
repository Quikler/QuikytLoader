using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.Mappers;

/// <summary>
/// Mapper for converting between Download entities and DTOs
/// </summary>
public static class DownloadMapper
{
    /// <summary>
    /// Maps DownloadRecord entity to DownloadHistoryRecordDto
    /// </summary>
    public static DownloadHistoryRecordDto ToDto(DownloadRecord record)
    {
        return new DownloadHistoryRecordDto
        {
            YouTubeId = record.YouTubeId.Value,
            VideoTitle = record.VideoTitle,
            DownloadedAt = record.DownloadedAt
        };
    }

    /// <summary>
    /// Maps DownloadHistoryRecordDto to DownloadRecord entity
    /// </summary>
    public static DownloadRecord ToEntity(DownloadHistoryRecordDto dto)
    {
        return new DownloadRecord
        {
            YouTubeId = new YouTubeId(dto.YouTubeId),
            VideoTitle = dto.VideoTitle,
            DownloadedAt = dto.DownloadedAt
        };
    }
}
