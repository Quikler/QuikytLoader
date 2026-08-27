using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.UseCases;
using QuikytLoader.Demo.Seed;
using QuikytLoader.Demo.Services;
using QuikytLoader.Demo.UseCases;

namespace QuikytLoader.Demo.DependencyInjection;

public static class DemoServiceCollectionExtensions
{
    public static IServiceCollection AddDemoServices(
        this IServiceCollection services)
    {
        services.AddSingleton<DemoMetadataSeed>();
        services.AddSingleton<DemoSubtitlesSeed>();

        services.AddSingleton<IYoutubeMetadataService, DemoYoutubeMetadataService>();
        services.AddSingleton<IYoutubeSubtitlesService, DemoYoutubeSubtitlesService>();
        services.AddSingleton<IDownloadAndSendUseCase, DemoDownloadAndSendUseCase>();

        return services;
    }
}
