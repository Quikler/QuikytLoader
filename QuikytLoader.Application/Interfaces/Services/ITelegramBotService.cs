using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for Telegram bot operations
/// </summary>
public interface ITelegramBotService : IDisposable
{
    /// <summary>
    /// Sends an audio file to the configured Telegram chat with thumbnail.
    /// Automatically initializes the bot on first use (lazy initialization).
    /// </summary>
    /// <param name="audioFilePath">Path to the audio file to send</param>
    /// <param name="thumbnailPath">Path to the thumbnail image (JPEG format required)</param>
    /// <returns>Result indicating success or error details</returns>
    Task<Result> SendAudioAsync(string audioFilePath, string? thumbnailPath = null);
}
