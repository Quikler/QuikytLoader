using QuikytLoader.Application.DTOs;

namespace QuikytLoader.Application.Interfaces.Repositories;

/// <summary>
/// Repository pattern for application settings persistence
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Loads settings from storage
    /// </summary>
    /// <returns>AppSettingsDto instance with current settings, or defaults if not found</returns>
    AppSettingsDto Load();

    /// <summary>
    /// Saves settings to storage
    /// </summary>
    /// <param name="settings">Settings to save</param>
    void Save(AppSettingsDto settings);
}
