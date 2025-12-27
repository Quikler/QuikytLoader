using System;
using Avalonia;
using Microsoft.Extensions.Hosting;
using QuikytLoader.Application.DependencyInjection;
using QuikytLoader.AvaloniaUI;
using QuikytLoader.AvaloniaUI.DependencyInjection;
using QuikytLoader.Infrastructure.DependencyInjection;

namespace QuikytLoader.Startup;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        BuildAvaloniaApp(host).StartWithClassicDesktopLifetime(args);
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices();
                services.AddAvaloniaUIServices();
            });
    }

    private static AppBuilder BuildAvaloniaApp(IHost host)
        => AppBuilder.Configure(() => new App(host.Services, host))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
