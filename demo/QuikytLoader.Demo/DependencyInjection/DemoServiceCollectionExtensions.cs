using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.Demo.Services;

namespace QuikytLoader.Demo.DependencyInjection;

public static class DemoServiceCollectionExtensions
{
    public static IServiceCollection AddDemoServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IYoutubeMetadataService, DemoYoutubeMetadataService>();
        services.AddSingleton<IYoutubeSubtitlesService, DemoYoutubeSubtitlesService>();

        services.AddSingleton<IYoutubeDownloadService, DemoYoutubeDownloadService>();
        services.AddSingleton<ITempDirectoryService, DemoTempDirectoryService>();
        services.AddSingleton<ITelegramBotService, DemoTelegramBotService>();

        // services.AddSingleton<IDownloadHistoryRepository, DemoDownloadHistoryRepository>();
        // P.S. Shouldn't mock IDownloadHistoryRepository because testing database is used:
        // services.AddTestingHistoryDatabase() call in Program.cs

        return services;
    }
}
