using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Manage application settings (load/save)
/// </summary>
public class ManageSettingsUseCase(ISettingsRepository settingsRepo)
{
    /// <summary>
    /// Loads application settings asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current settings or defaults if not found</returns>
    public Task<AppSettingsDto> LoadSettingsAsync(CancellationToken cancellationToken = default)
        => settingsRepo.LoadAsync(cancellationToken);

    /// <summary>
    /// Saves application settings asynchronously
    /// </summary>
    /// <param name="settings">Settings to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task SaveSettingsAsync(AppSettingsDto settings, CancellationToken cancellationToken = default)
        => settingsRepo.SaveAsync(settings, cancellationToken);
}
