using QuikytLoader.Application.DTOs;

namespace QuikytLoader.Application.Interfaces.Settings;

/// <summary>
/// Interface for user settings persistence (JSON file storage)
/// </summary>
public interface IUserSettings
{
    /// <summary>
    /// Loads settings from storage
    /// </summary>
    /// <returns>UserSettingsDto instance with current settings, or defaults if not found</returns>
    UserSettingsDto Load();

    /// <summary>
    /// Saves settings to storage
    /// </summary>
    /// <param name="settings">Settings to save</param>
    void Save(UserSettingsDto settings);
}
