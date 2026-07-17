using System.Text.Json;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Infrastructure.Persistence.Json;

namespace QuikytLoader.Infrastructure.Persistence.Settings;

/// <summary>
/// User settings persistence using JSON file storage
/// Follows XDG Base Directory specification on Linux (~/.config/QuikytLoader)
/// </summary>
internal class UserSettings : IUserSettings
{
    private readonly string _settingsPath;

    public UserSettings()
    {
        // Use XDG_CONFIG_HOME or fallback to ~/.config on Linux
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuikytLoader"
        );

        Directory.CreateDirectory(configDir);
        _settingsPath = Path.Combine(configDir, "settings.json");

        // Set restrictive permissions on Linux (user read/write only - mode 600)
        if (OperatingSystem.IsLinux() && File.Exists(_settingsPath))
        {
            File.SetUnixFileMode(_settingsPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    /// <summary>
    /// Loads settings from JSON file
    /// Creates default settings file if it doesn't exist
    /// Returns defaults if file is corrupted
    /// </summary>
    public UserSettingsDto Load()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaultSettings = new UserSettingsDto();
            Save(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.UserSettingsDto) ?? new UserSettingsDto();
        }
        catch (JsonException)
        {
            // Corrupted file, return defaults
            return new UserSettingsDto();
        }
    }

    /// <summary>
    /// Saves settings to JSON file using atomic write operation
    /// Writes to temporary file first, then renames to prevent corruption
    /// Sets restrictive file permissions on Linux (mode 600)
    /// </summary>
    public void Save(UserSettingsDto settings)
    {
        var json = JsonSerializer.Serialize(settings, AppJsonSerializerContext.Default.UserSettingsDto);

        // Atomic write: write to temp file, then rename
        var tempPath = _settingsPath + ".tmp";
        File.WriteAllText(tempPath, json);

        if (OperatingSystem.IsLinux())
        {
            File.SetUnixFileMode(tempPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }

        File.Move(tempPath, _settingsPath, overwrite: true);
    }
}
