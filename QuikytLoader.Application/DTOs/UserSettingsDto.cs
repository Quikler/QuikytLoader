namespace QuikytLoader.Application.DTOs;

/// <summary>
/// Data transfer object for user settings (Telegram configuration)
/// </summary>
public class UserSettingsDto
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
