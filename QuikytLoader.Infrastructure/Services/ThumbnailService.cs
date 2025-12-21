using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace QuikytLoader.Infrastructure.Services;

/// <summary>
/// Service for processing thumbnails to meet platform-specific requirements
/// </summary>
internal class ThumbnailService : IThumbnailService
{
    private const int TelegramMaxThumbnailDimension = 320;

    /// <summary>
    /// Processes thumbnail to meet Telegram requirements: 320x320 max, JPEG format, square crop
    /// </summary>
    public Result ProcessForTelegram(string thumbnailPath)
    {
        // Validate file exists
        if (!File.Exists(thumbnailPath))
        {
            return Result.Failure(Errors.Thumbnail.FileNotFound(thumbnailPath));
        }

        try
        {
            using var image = Image.Load(thumbnailPath);

            // Check if processing is needed
            var maxDimension = Math.Max(image.Width, image.Height);
            if (maxDimension <= TelegramMaxThumbnailDimension && image.Width == image.Height)
            {
                // Already within limits and square, no processing needed
                return Result.Success();
            }

            // Crop to square (center crop)
            var minDimension = Math.Min(image.Width, image.Height);
            var cropX = (image.Width - minDimension) / 2;
            var cropY = (image.Height - minDimension) / 2;

            image.Mutate(x => x
                .Crop(new Rectangle(cropX, cropY, minDimension, minDimension))
                .Resize(new ResizeOptions
                {
                    Size = new Size(TelegramMaxThumbnailDimension, TelegramMaxThumbnailDimension),
                    Mode = ResizeMode.Max // Only resize if larger than max dimension
                }));

            // Save back to the same file as JPEG
            image.SaveAsJpeg(thumbnailPath);

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process thumbnail: {ex.Message}");
            // Non-critical error, continue without processed thumbnail
            return Result.Failure(Errors.Thumbnail.ProcessingFailed(ex.Message));
        }
    }
}