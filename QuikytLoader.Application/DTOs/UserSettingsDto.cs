using System.Text.Json.Serialization;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Application.DTOs;

/// <summary>
/// Data transfer object for user settings (Telegram configuration)
/// </summary>
public record UserSettingsDto
{
    /// <summary>
    /// Language detection for auto subtitles
    /// </summary>
    public bool LanguageDetectionForAutoSubtitles { get; set; }

    /// <summary>
    /// User's preferred application theme. Stored as string in JSON - not integer (e.g., "Light" instead of 0)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ThemePreference>))]
    public ThemePreference ThemePreference { get; set; }

    /// <summary>
    /// Telegram bot token from @BotFather
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// Telegram chat ID where files will be sent
    /// </summary>
    public string ChatId { get; set; } = string.Empty;
}
