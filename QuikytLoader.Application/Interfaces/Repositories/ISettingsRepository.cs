using QuikytLoader.Application.DTOs;

namespace QuikytLoader.Application.Interfaces.Repositories;

/// <summary>
/// Repository pattern for application settings persistence
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Loads settings from storage asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AppSettingsDto instance with current settings, or defaults if not found</returns>
    Task<AppSettingsDto> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves settings to storage asynchronously
    /// </summary>
    /// <param name="settings">Settings to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveAsync(AppSettingsDto settings, CancellationToken cancellationToken = default);
}
