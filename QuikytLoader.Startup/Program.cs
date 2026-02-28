using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI;
using QuikytLoader.AvaloniaUI.DependencyInjection;
using QuikytLoader.Application.DependencyInjection;
using QuikytLoader.Infrastructure.DependencyInjection;

namespace QuikytLoader.Startup;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();
        services.AddInfrastructureServices();
        services.AddAvaloniaUIServices();

        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
        BuildAvaloniaApp(serviceProvider).StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
