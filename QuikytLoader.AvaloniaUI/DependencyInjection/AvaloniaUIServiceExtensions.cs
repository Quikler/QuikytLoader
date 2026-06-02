using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

        // For testing purposes we skip the whole Download
        // and send to Telegram use case, it means we are
        // not using yt-dlp or sending anything to Telegram
#if DEBUG
        services.AddSingleton(_ =>
            new DownloadQueueManager(async (_, _) =>
                {
                    // Small delay for testing
                    await Task.Delay(1000);
                    return Result.Success();
                }));
#else
        services.AddSingleton(sp =>
             new DownloadQueueManager(async (item, ct) =>
                await sp.GetRequiredService<DownloadAndSendUseCase>()
                    .ExecuteAsync(item.Url, item.CustomTitle,
                        new Progress<double>(value => item.Progress = value), ct)
                ));
#endif

        services.AddSingleton<QueueAdditionService>();

        services.AddTransient<AppViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<QueueListViewModel>();

        return services;
    }
}
