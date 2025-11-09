using System;
using System.Threading.Tasks;

namespace QuikytLoader.Services;

/// <summary>
/// Interface for Telegram bot operations
/// Implements IAsyncDisposable for proper resource cleanup
/// </summary>
public interface ITelegramBotService : IAsyncDisposable
{
    /// <summary>
    /// Sends an audio file to the configured Telegram chat
    /// Automatically initializes the bot on first use (lazy initialization)
    /// </summary>
    /// <param name="filePath">Path to the audio file to send</param>
    Task SendAudioAsync(string filePath);
}
