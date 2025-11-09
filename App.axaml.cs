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
            var mainViewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
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
                services.AddTransient<MainWindowViewModel>();

                // Register Services
                services.AddSingleton<IYouTubeDownloadService, YouTubeDownloadService>();
                // services.AddSingleton<ITelegramBotService, TelegramBotService>();
            });
    }
}
