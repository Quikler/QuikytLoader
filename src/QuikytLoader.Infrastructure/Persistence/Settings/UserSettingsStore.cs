using System.Text.Json;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Infrastructure.Persistence.Json;

namespace QuikytLoader.Infrastructure.Persistence.Settings;

internal class UserSettingsStore : IUserSettingsStore
{
    private readonly string _settingsPath;

    public UserSettingsStore()
    {
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuikytLoader");

        Directory.CreateDirectory(configDir);

        _settingsPath = Path.Combine(configDir, "settings.json");
    }

    public UserSettingsDto Load()
    {
        if (!File.Exists(_settingsPath))
            return new UserSettingsDto();

        try
        {
            var json = File.ReadAllText(_settingsPath);

            return JsonSerializer.Deserialize(
                json,
                AppJsonSerializerContext.Default.UserSettingsDto)
                ?? new UserSettingsDto();
        }
        catch (JsonException)
        {
            return new UserSettingsDto();
        }
    }

    public void Save(UserSettingsDto settings)
    {
        var json = JsonSerializer.Serialize(
            settings,
            AppJsonSerializerContext.Default.UserSettingsDto);

        var tempPath = _settingsPath + ".tmp";

        File.WriteAllText(tempPath, json);
        File.Move(tempPath, _settingsPath, overwrite: true);
    }
}
