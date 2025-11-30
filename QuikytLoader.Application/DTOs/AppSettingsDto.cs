namespace QuikytLoader.Application.DTOs;

/// <summary>
/// Data transfer object for application settings
/// </summary>
public class AppSettingsDto
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
