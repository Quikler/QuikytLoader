using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuikytLoader.Application.DependencyInjection;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.AvaloniaUI.Views;
using QuikytLoader.Infrastructure.DependencyInjection;

namespace QuikytLoader.AvaloniaUI;

public partial class App : Avalonia.Application
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

            // Initialize SettingsViewModel to load settings asynchronously
            var settingsViewModel = _host.Services.GetRequiredService<SettingsViewModel>();
            _ = settingsViewModel.InitializeAsync(); // Fire-and-forget for framework initialization

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

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register layer services using extension methods
                services.AddApplicationServices();
                services.AddInfrastructureServices();

                // Register ViewModels
                services.AddTransient<AppViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<SettingsViewModel>();
            });
    }
}
