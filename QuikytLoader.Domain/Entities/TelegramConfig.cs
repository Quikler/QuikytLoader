namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing Telegram bot configuration
/// </summary>
public class TelegramConfig
{
    /// <summary>
    /// Bot token obtained from @BotFather
    /// </summary>
    public required string BotToken { get; init; }

    /// <summary>
    /// Chat ID obtained from @userinfobot
    /// </summary>
    public required string ChatId { get; init; }

    /// <summary>
    /// Validates that the configuration has required values
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BotToken) && !string.IsNullOrWhiteSpace(ChatId);
    }
}
