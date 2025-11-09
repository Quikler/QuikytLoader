namespace QuikytLoader.Models;

/// <summary>
/// Application settings model
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Telegram bot token from @BotFather
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// Telegram chat ID where files will be sent
    /// </summary>
    public string ChatId { get; set; } = string.Empty;
}
