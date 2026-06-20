namespace QuikytLoader.Application.DTOs;

public record DownloadHistoryDto(string YoutubeVideoId, string VideoTitle, DateTime DownloadedAt);
