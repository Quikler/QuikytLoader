using QuikytLoader.Application.DTOs;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.Application.Mappers;

/// <summary>
/// Mapper for converting between Settings entities and DTOs
/// </summary>
public static class SettingsMapper
{
    /// <summary>
    /// Maps TelegramConfig entity to AppSettingsDto
    /// </summary>
    public static AppSettingsDto ToDto(TelegramConfig config)
    {
        return new AppSettingsDto
        {
            BotToken = config.BotToken,
            ChatId = config.ChatId
        };
    }

    /// <summary>
    /// Maps AppSettingsDto to TelegramConfig entity
    /// </summary>
    public static TelegramConfig ToEntity(AppSettingsDto dto)
    {
        return new TelegramConfig
        {
            BotToken = dto.BotToken,
            ChatId = dto.ChatId
        };
    }
}
