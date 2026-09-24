using System;
using System.IO;
using Avalonia;
using QuikytLoader.Application;

namespace QuikytLoader.AvaloniaUI;

public sealed class AvaloniaApplication : IApplication
{
    private readonly IServiceProvider _services;

    public Guid InstanceId { get; }

    public string TempInstanceDirectory { get; }

    public AvaloniaApplication(IServiceProvider services)
    {
        _services = services;

        InstanceId = Guid.NewGuid();
        TempInstanceDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader", InstanceId.ToString());
    }

    public void Run(string[] args)
    {
        AppBuilder.Configure(() => new App(_services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }
}
