using System;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI.DependencyInjection;
using QuikytLoader.Application.DependencyInjection;
using QuikytLoader.Infrastructure.DependencyInjection;
using QuikytLoader.Application;

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

        serviceProvider
            .GetRequiredService<IApplication>()
            .Run(args);
    }
}
