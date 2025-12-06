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
        services.AddTransient<DownloadAndSendUseCase>();
        services.AddTransient<CheckDuplicateUseCase>();
        services.AddTransient<GetVideoInfoUseCase>();
        services.AddTransient<ManageSettingsUseCase>();

        return services;
    }
}
