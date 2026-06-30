using System;
using Avalonia;
using QuikytLoader.Application;

namespace QuikytLoader.AvaloniaUI;

public sealed class AvaloniaApplication(IServiceProvider services) : IApplication
{
    public void Run(string[] args)
    {
        AppBuilder.Configure(() => new App(services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }
}
