using System;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.ViewModels;

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
        services.AddSingleton<IUiNotificationService, UiNotificationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeApplier, ThemeApplier>();

        services.AddSingleton(sp =>
             new DownloadQueueManager(async (item, ct) =>
                await sp.GetRequiredService<DownloadAndSendUseCase>()
                    .ExecuteAsync(item.VideoMetadata.Url, item.CustomTitle,
                        new Progress<double>(value => item.Progress = value), ct)
                ));

        services.AddSingleton<QueueAdditionService>();

        services.AddSingleton<AppViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SettingsViewModel>();

        services.AddSingleton<YoutubeUrlInputCardViewModel>();
        services.AddSingleton<QueueListViewModel>();
        services.AddSingleton<MessageInfoViewModel>();

        return services;
    }
}
