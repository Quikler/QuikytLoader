using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI.Services;
using QuikytLoader.AvaloniaUI.Validators;
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
        services.AddSingleton<YoutubeUrlValidator>();

        services.AddSingleton<IUiNotificationService, UiNotificationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeApplier, ThemeApplier>();

        services.AddSingleton<DownloadQueueManager>();

        services.AddSingleton<AppViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<SettingsViewModel>();

        services.AddSingleton<YoutubeUrlInputCardViewModel>();
        services.AddSingleton<QueueListViewModel>();
        services.AddSingleton<MessageInfoViewModel>();

        return services;
    }
}
