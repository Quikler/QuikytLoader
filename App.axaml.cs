using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuikytLoader.Services;
using QuikytLoader.ViewModels;
using QuikytLoader.Views;

namespace QuikytLoader;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _host = CreateHostBuilder().Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var appViewModel = _host.Services.GetRequiredService<AppViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = appViewModel
            };

            // Cleanup on application exit - DI container will dispose all services
            desktop.ShutdownRequested += async (s, e) =>
            {
                if (_host != null)
                {
                    await _host.StopAsync(); // Disposes async disposable services
                    _host.Dispose();
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register ViewModels
                services.AddTransient<AppViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<MainWindowViewModel>();

                // Register Services
                services.AddSingleton<ISettingsManager, SettingsManager>();
                services.AddSingleton<IYouTubeDownloadService, YouTubeDownloadService>();
                services.AddSingleton<ITelegramBotService, TelegramBotService>();
            });
    }
}
