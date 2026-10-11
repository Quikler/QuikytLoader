namespace QuikytLoader.Application.Interfaces.Services;

public record PreparedMedia(
    string FinalMp3Path,
    string FinalThumbnailPath,
    string OriginalMp3Path,
    string OriginalThumbnailPath);

public interface ITelegramMediaService
{
    public PreparedMedia PrepareMedia(
        string mediaDirectory,
        string tempMp3FilePath,
        string tempThumbnailFilePath);

    public PreparedMedia PrepareMediaWithTitle(
        string mediaDirectory,
        string tempMp3FilePath,
        string tempThumbnailFilePath,
        string sanitizedCustomTitle);

    void Rollback(PreparedMedia media);
}
