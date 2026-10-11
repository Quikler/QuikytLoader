using QuikytLoader.Application.Interfaces.Services;

namespace QuikytLoader.Infrastructure.Telegram;

public class TelegramMediaService : ITelegramMediaService
{
    public PreparedMedia PrepareMedia(
        string mediaDirectory,
        string sourceMp3,
        string sourceThumbnail)
    {
        var normalizedThumbnailPath = ConvertThumbnailForTelegram(
            mediaDirectory,
            sourceThumbnail);

        return new PreparedMedia(
            sourceMp3,
            normalizedThumbnailPath,
            sourceMp3,
            normalizedThumbnailPath);
    }

    public PreparedMedia PrepareMediaWithTitle(
        string mediaDirectory,
        string sourceMp3,
        string sourceThumbnail,
        string sanitizedCustomTitle)
    {
        var normalizedThumbnailPath = ConvertThumbnailForTelegram(
            mediaDirectory,
            sourceThumbnail);

        // Rename with custom title (these DO need rollback)
        var destMp3 = Path.Combine(mediaDirectory, sanitizedCustomTitle + Path.GetExtension(sourceMp3));
        File.Move(sourceMp3, destMp3);

        var destThumbnail = Path.Combine(mediaDirectory, sanitizedCustomTitle + Path.GetExtension(sourceThumbnail));
        File.Move(normalizedThumbnailPath, destThumbnail);

        return new PreparedMedia(
            destMp3,
            destThumbnail,
            sourceMp3,
            normalizedThumbnailPath);
    }

    public void Rollback(PreparedMedia media)
    {
        File.Move(media.FinalMp3Path, media.OriginalMp3Path, overwrite: true);
        File.Move(media.FinalThumbnailPath, media.OriginalThumbnailPath, overwrite: true);
    }

    private string ConvertThumbnailForTelegram(
        string mediaDirectory,
        string sourceThumbnail)
    {
        // Convert to .jpeg for Telegram compatibility
        var normalizedThumbnailPath = Path.Combine(
            mediaDirectory,
            $"{Path.GetFileNameWithoutExtension(sourceThumbnail)}.jpeg");
        File.Move(sourceThumbnail, normalizedThumbnailPath, overwrite: true);
        return normalizedThumbnailPath;
    }
}
