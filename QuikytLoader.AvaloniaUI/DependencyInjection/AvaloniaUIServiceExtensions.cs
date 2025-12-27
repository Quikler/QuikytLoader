using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IDialogService, DialogService>();

        services.AddTransient<AppViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
