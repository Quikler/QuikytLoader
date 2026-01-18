using System;
using System.Threading.Tasks;
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
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        try
        {
            await host.StartAsync();
            BuildAvaloniaApp(host).StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            await host.StopAsync();
            host.Dispose();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime(options => options.SuppressStatusMessages = true)
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructureServices();
                services.AddAvaloniaUIServices();
            });
    }

    private static AppBuilder BuildAvaloniaApp(IHost host)
        => AppBuilder.Configure(() => new App(host.Services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
