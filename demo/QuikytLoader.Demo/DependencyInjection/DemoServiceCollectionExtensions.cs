using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Repositories;
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
        services.AddSingleton<IDownloadHistoryRepository, DemoDownloadHistoryRepository>();
        services.AddSingleton<ITelegramBotService, DemoTelegramBotService>();

        return services;
    }
}
