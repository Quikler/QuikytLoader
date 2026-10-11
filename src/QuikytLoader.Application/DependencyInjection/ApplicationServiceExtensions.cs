using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Services.Queue;
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
        // Queue services
        services.AddSingleton<IDownloadQueue, DownloadQueue>();
        services.AddSingleton<IDownloadQueueProcessor, DownloadQueueProcessor>();

        // Register Use Cases
        services.AddSingleton<IAddToQueueUseCase, AddToQueueUseCase>();
        services.AddSingleton<IDownloadAndSendUseCase, DownloadAndSendUseCase>();
        services.AddSingleton<IFetchManualSubtitlesUseCase, FetchManualSubtitlesUseCase>();
        services.AddSingleton<IFetchAutoSubtitlesUseCase, FetchAutoSubtitlesUseCase>();
        services.AddSingleton<ICancelSubtitlesUseCase, CancelSubtitlesUseCase>();
        services.AddSingleton<IFindExistingDownloadUseCase, FindExistingDownloadUseCase>();

        return services;
    }
}
