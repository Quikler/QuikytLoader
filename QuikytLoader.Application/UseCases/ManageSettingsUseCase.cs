using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Manage application settings (load/save)
/// </summary>
public class ManageSettingsUseCase
{
    private readonly ISettingsRepository _settingsRepo;

    public ManageSettingsUseCase(ISettingsRepository settingsRepo)
    {
        _settingsRepo = settingsRepo;
    }

    /// <summary>
    /// Loads application settings
    /// </summary>
    /// <returns>Current settings or defaults if not found</returns>
    public AppSettingsDto LoadSettings()
    {
        return _settingsRepo.Load();
    }

    /// <summary>
    /// Saves application settings
    /// </summary>
    /// <param name="settings">Settings to save</param>
    public void SaveSettings(AppSettingsDto settings)
    {
        _settingsRepo.Save(settings);
    }
}
