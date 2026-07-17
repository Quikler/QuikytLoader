using System;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI.DependencyInjection;
using QuikytLoader.Application.DependencyInjection;
using QuikytLoader.Infrastructure.DependencyInjection;
using QuikytLoader.Application;
using QuikytLoader.Demo.DependencyInjection;

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
#if DEBUG
        services.AddDemoServices();
#endif

        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        serviceProvider
            .GetRequiredService<IApplication>()
            .Run(args);
    }
}
