using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Repositories;

namespace QuikytLoader.Application.UseCases;

public class ManageSettingsUseCase(ISettingsRepository settingsRepo)
{
    public Task<AppSettingsDto> LoadSettingsAsync(CancellationToken cancellationToken = default)
        => settingsRepo.LoadAsync(cancellationToken);

    public Task SaveSettingsAsync(AppSettingsDto settings, CancellationToken cancellationToken = default)
        => settingsRepo.SaveAsync(settings, cancellationToken);
}
