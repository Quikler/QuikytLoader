using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.DTOs;

public record PlaylistVideoDto(DownloadSource Source, VideoMetadata Metadata);
