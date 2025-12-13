using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for Telegram bot operations
/// Implements IAsyncDisposable for proper resource cleanup
/// </summary>
public interface ITelegramBotService : IAsyncDisposable
{
    /// <summary>
    /// Sends an audio file to the configured Telegram chat with optional thumbnail.
    /// Automatically initializes the bot on first use (lazy initialization).
    /// Returns a Result indicating success or containing error details on failure.
    /// </summary>
    /// <param name="audioFilePath">Path to the audio file to send</param>
    /// <param name="thumbnailPath">Optional path to the thumbnail image (JPEG format required)</param>
    /// <returns>Result indicating success or error details</returns>
    Task<Result> SendAudioAsync(string audioFilePath, string? thumbnailPath = null);
}
