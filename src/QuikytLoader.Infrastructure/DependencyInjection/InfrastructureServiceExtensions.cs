using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Parsers;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Application.Interfaces.UseCases;
using QuikytLoader.Infrastructure.LanguageIdentification;
using QuikytLoader.Infrastructure.Parsers;
using QuikytLoader.Infrastructure.Persistence;
using QuikytLoader.Infrastructure.Persistence.Repositories;
using QuikytLoader.Infrastructure.Persistence.Settings;
using QuikytLoader.Infrastructure.Persistence.Temp;
using QuikytLoader.Infrastructure.Queue;
using QuikytLoader.Infrastructure.Services;
using QuikytLoader.Infrastructure.Telegram;
using QuikytLoader.Infrastructure.Youtube;
using QuikytLoader.Infrastructure.Youtube.ACL.Services;
using QuikytLoader.Infrastructure.Youtube.Subtitles;
using QuikytLoader.Infrastructure.Youtube.YtDlp;

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
        // Language identification
        services.AddSingleton<ILanguageIdentifier, LanguageIdentifier>();

        // Youtube services
        services.AddSingleton<IYoutubeDownloadService, YoutubeDownloadService>();

        services.AddSingleton<IYoutubeMetadataService, YoutubeMetadataService>();
        services.AddSingleton<IYoutubeSubtitlesService, YoutubeSubtitlesService>();

        services.AddSingleton<IYtDlpProcessClient, YtDlpProcessClient>();
        services.AddSingleton<IYtDlpAcl, YtDlpAcl>();

        services.AddSingleton<IYoutubeVideoIdParser, YoutubeVideoIdParser>();
        services.AddSingleton<IYoutubePlaylistIdParser, YoutubePlaylistIdParser>();

        // Thumbnail service
        services.AddSingleton<IThumbnailService, ThumbnailService>();

        // Telegram services
        services.AddSingleton<ITelegramBotService, TelegramBotService>();

        // Queue services
        services.AddSingleton<IDownloadQueue, DownloadQueue>();
        services.AddSingleton<IDownloadQueueProcessor, DownloadQueueProcessor>(
            s => new DownloadQueueProcessor(
                s.GetRequiredService<IDownloadQueue>(),
                (queueItem, progress, ct) =>
                    s.GetRequiredService<IDownloadAndSendUseCase>()
                        .ExecuteAsync(queueItem.Source, queueItem.CustomTitle, progress, ct)
            )
        );

        // Persistence
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<IDownloadHistoryRepository, DownloadHistoryRepository>();
        services.AddSingleton<IUserSettings, UserSettings>();
        services.AddSingleton<ITempDirectoryService, TempDirectoryService>();

        return services;
    }
}
