using QuikytLoader.Application.Interfaces.Services;

namespace QuikytLoader.Demo.Services;

public class DemoTelegramMediaService : ITelegramMediaService
{
    public PreparedMedia PrepareMedia(
        string mediaDirectory,
        string tempMp3FilePath,
        string tempThumbnailFilePath)
            => CreatePreparedMedia(
                tempMp3FilePath,
                tempThumbnailFilePath);

    public PreparedMedia PrepareMediaWithTitle(
        string mediaDirectory,
        string tempMp3FilePath,
        string tempThumbnailFilePath,
        string sanitizedCustomTitle)
            => CreatePreparedMediaWithTitle(
                mediaDirectory,
                tempMp3FilePath,
                tempThumbnailFilePath,
                sanitizedCustomTitle);

    public void Rollback(PreparedMedia media) { }
}
