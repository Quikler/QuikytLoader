using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Application.UseCases;
using QuikytLoader.Demo.Services;
using QuikytLoader.Demo.UseCases;

namespace QuikytLoader.Demo.DependencyInjection;

public static class DemoServiceCollectionExtensions
{
    public static IServiceCollection AddDemoServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IYoutubeMetadataService, DemoYoutubeMetadataService>();
        services.AddSingleton<IYoutubeSubtitlesService, DemoYoutubeSubtitlesService>();
        services.AddSingleton<IDownloadAndSendUseCase, DemoDownloadAndSendUseCase>();

        return services;
    }
}
