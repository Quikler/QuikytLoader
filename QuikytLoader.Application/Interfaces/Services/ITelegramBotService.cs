namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for Telegram bot operations
/// Implements IAsyncDisposable for proper resource cleanup
/// </summary>
public interface ITelegramBotService : IAsyncDisposable
{
    /// <summary>
    /// Sends an audio file to the configured Telegram chat with optional thumbnail
    /// Automatically initializes the bot on first use (lazy initialization)
    /// </summary>
    /// <param name="audioFilePath">Path to the audio file to send</param>
    /// <param name="thumbnailPath">Optional path to the thumbnail image (JPEG format required)</param>
    Task SendAudioAsync(string audioFilePath, string? thumbnailPath = null);
}
