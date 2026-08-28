using System.Text.Json.Serialization;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Application.DTOs;

/// <summary>
/// Data transfer object for user settings (Telegram configuration)
/// </summary>
public record UserSettingsDto
{
    /// <summary>
    /// Auto subtitles option
    /// </summary>
    public AutoSubtitlesOption AutoSubtitlesOption { get; init; }

    /// <summary>
    /// User's preferred application theme. Stored as string in JSON - not integer (e.g., "Light" instead of 0)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ThemePreference>))]
    public ThemePreference ThemePreference { get; init; }

    /// <summary>
    /// Telegram bot token from @BotFather
    /// </summary>
    public string BotToken { get; init; } = string.Empty;

    /// <summary>
    /// Telegram chat ID where files will be sent
    /// </summary>
    public string ChatId { get; init; } = string.Empty;
}
