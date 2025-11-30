using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
//using QuikytLoader.Services;
using QuikytLoader.ViewModels;
using QuikytLoader.Views;

namespace QuikytLoader;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            });

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();

        // Register Services (we'll add these later)
        // services.AddSingleton<IYouTubeDownloadService, YouTubeDownloadService>();
        // services.AddSingleton<ITelegramBotService, TelegramBotService>();
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
