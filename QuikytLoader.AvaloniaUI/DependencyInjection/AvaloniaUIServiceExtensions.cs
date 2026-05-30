using System;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.AvaloniaUI.DependencyInjection;

/// <summary>
/// Extension methods for registering AvaloniaUI layer services
/// </summary>
public static class AvaloniaUIServiceExtensions
{
    /// <summary>
    /// Registers all AvaloniaUI layer services (ViewModels)
    /// </summary>
    public static IServiceCollection AddAvaloniaUIServices(this IServiceCollection services)
    {
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeApplier, ThemeApplier>();

        // Adding DownloadQueueManager with processQueueItem delegate
        services.AddSingleton(sp =>
        {
            return new DownloadQueueManager(async (item, ct) =>
            {
#if DEBUG
                // Testing purpose
                if (item.Url == TestConstants.YoutubeTestVideoUrl)
                {
                    return Result.Success();
                }
#endif

                return await sp.GetRequiredService<DownloadAndSendUseCase>()
                    .ExecuteAsync(item.Url, item.CustomTitle,
                        new Progress<double>(value => item.Progress = value), ct);
            });
        });

        services.AddSingleton<QueueAdditionService>();


        services.AddTransient<AppViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
