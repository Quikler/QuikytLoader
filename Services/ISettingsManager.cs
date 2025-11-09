using QuikytLoader.Models;

namespace QuikytLoader.Services;

/// <summary>
/// Interface for managing application settings persistence
/// </summary>
public interface ISettingsManager
{
    /// <summary>
    /// Loads settings from storage
    /// </summary>
    /// <returns>AppSettings instance with current settings, or defaults if not found</returns>
    AppSettings Load();

    /// <summary>
    /// Saves settings to storage
    /// </summary>
    /// <param name="settings">Settings to save</param>
    void Save(AppSettings settings);
}
