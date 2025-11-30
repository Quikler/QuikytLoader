using System.Text.Json;
using System.Text.Json.Serialization;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;

namespace QuikytLoader.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for application settings persistence using JSON file storage
/// Follows XDG Base Directory specification on Linux (~/.config/QuikytLoader)
/// </summary>
internal class SettingsRepository : ISettingsRepository
{
    private readonly string _settingsPath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SettingsRepository()
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
    public AppSettingsDto Load()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaultSettings = new AppSettingsDto();
            Save(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettingsDto>(json) ?? new AppSettingsDto();
        }
        catch (JsonException)
        {
            // Corrupted file, return defaults
            return new AppSettingsDto();
        }
    }

    /// <summary>
    /// Saves settings to JSON file using atomic write operation
    /// Writes to temporary file first, then renames to prevent corruption
    /// Sets restrictive file permissions on Linux (mode 600)
    /// </summary>
    public void Save(AppSettingsDto settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);

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
