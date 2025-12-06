using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Infrastructure.Persistence;
using QuikytLoader.Infrastructure.Persistence.Repositories;
using QuikytLoader.Infrastructure.Telegram;
using QuikytLoader.Infrastructure.YouTube;

namespace QuikytLoader.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers all Infrastructure layer services (implementations of Application interfaces)
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // YouTube services
        services.AddSingleton<IYouTubeDownloadService, YouTubeDownloadService>();
        services.AddSingleton<IYoutubeExtractor, YoutubeExtractor>();

        // Telegram services
        services.AddSingleton<ITelegramBotService, TelegramBotService>();

        // Persistence
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<IDownloadHistoryRepository, DownloadHistoryRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        return services;
    }
}
