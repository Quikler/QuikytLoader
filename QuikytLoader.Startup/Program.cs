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
    /// <summary>
    /// Application entry point that builds the host and starts the Avalonia Classic Desktop lifetime.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to the host builder and Avalonia application lifetime.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        BuildAvaloniaApp(host).StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Create an IHostBuilder configured with application, infrastructure, and Avalonia UI services.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to the host builder.</param>
    /// <returns>An IHostBuilder configured with default settings and the application's services registered.</returns>
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

    /// <summary>
            /// Creates an Avalonia AppBuilder configured to construct the application's App using the provided host.
            /// </summary>
            /// <param name="host">The application host whose service provider is passed into the App.</param>
            /// <returns>An AppBuilder configured for the current platform and ready to start the Avalonia application.</returns>
            private static AppBuilder BuildAvaloniaApp(IHost host)
        => AppBuilder.Configure(() => new App(host.Services, host))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}