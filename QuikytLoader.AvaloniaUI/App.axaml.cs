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
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

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
