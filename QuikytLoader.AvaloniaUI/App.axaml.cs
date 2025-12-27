using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.AvaloniaUI.Views;

namespace QuikytLoader.AvaloniaUI;

public partial class App(IServiceProvider serviceProvider, IHost host) : Avalonia.Application
{
    /// <summary>
/// Loads the application's XAML resources and visual tree defined for this Application instance.
/// </summary>
public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Initializes the desktop application's main window and registers shutdown handling after Avalonia framework initialization.
    /// </summary>
    /// <remarks>
    /// If the application lifetime is a classic desktop lifetime, this method sets the desktop MainWindow with its DataContext
    /// resolved from the provided service provider and subscribes to the desktop ShutdownRequested event to stop and dispose the provided host.
    /// </remarks>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<AppViewModel>()
            };

            // Cleanup on application exit - DI container will dispose all services
            desktop.ShutdownRequested += async (s, e) =>
            {
                await host.StopAsync(); // Disposes async disposable services
                host.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}