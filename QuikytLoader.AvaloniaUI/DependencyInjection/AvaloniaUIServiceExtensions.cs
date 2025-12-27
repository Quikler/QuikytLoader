using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI.ViewModels;

namespace QuikytLoader.AvaloniaUI.DependencyInjection;

/// <summary>
/// Extension methods for registering AvaloniaUI layer services
/// </summary>
public static class AvaloniaUIServiceExtensions
{
    /// <summary>
    /// Registers all AvaloniaUI layer services (ViewModels)
    /// <summary>
    /// Registers Avalonia UI view-model services into the given service collection.
    /// </summary>
    /// <param name="services">The service collection to add the view-model registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance after registering the view-models.</returns>
    public static IServiceCollection AddAvaloniaUIServices(this IServiceCollection services)
    {
        // Register ViewModels
        services.AddTransient<AppViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}