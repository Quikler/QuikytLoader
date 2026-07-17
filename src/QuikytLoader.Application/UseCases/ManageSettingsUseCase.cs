using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Settings;

namespace QuikytLoader.Application.UseCases;

public class ManageSettingsUseCase(IUserSettings userSettings)
{
    public UserSettingsDto LoadSettings() => userSettings.Load();
    public void SaveSettings(UserSettingsDto settings) => userSettings.Save(settings);
}
