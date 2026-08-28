using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Services;

/// <summary>
/// Interface for Telegram bot operations
/// </summary>
public interface ITelegramBotService : IDisposable
{
    Task<Result> SendAudioAsync(string mp3FilePath, string thumbnailFilePath);
}
