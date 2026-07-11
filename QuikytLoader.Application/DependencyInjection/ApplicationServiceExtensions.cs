using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.UseCases;

namespace QuikytLoader.Application.DependencyInjection;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registers all Application layer services (Use Cases)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Use Cases
        services.AddSingleton<AddToQueueUseCase>();
        services.AddSingleton<DownloadAndSendUseCase>();
        services.AddSingleton<FetchSubtitlesUseCase>();
        services.AddSingleton<CancelSubtitlesUseCase>();
        services.AddSingleton<FindExistingDownloadUseCase>();
        services.AddSingleton<ManageSettingsUseCase>();

        return services;
    }
}
