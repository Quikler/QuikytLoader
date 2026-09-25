using System;
using System.IO;
using Avalonia;
using QuikytLoader.Application;

namespace QuikytLoader.AvaloniaUI;

public sealed class AvaloniaApplication : IApplication
{
    private readonly IServiceProvider _services;
    private readonly string _tempDirectory;

#pragma warning disable IDE0052
    // We need to hold a reference to a lock file,
    // so it's not garbage collected
    private FileStream? _lockFileStream;
#pragma warning restore IDE0052

    public Guid InstanceId { get; }

    public string TempInstanceDirectory { get; }

    public AvaloniaApplication(IServiceProvider services)
    {
        _services = services;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

        InstanceId = Guid.NewGuid();
        TempInstanceDirectory = Path.Combine(_tempDirectory, InstanceId.ToString());
    }

    public void Run(string[] args)
    {
        CreateInstanceLock();

        AppBuilder.Configure(() => new App(_services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }

    private void CreateInstanceLock()
    {
        Directory.CreateDirectory(TempInstanceDirectory);
        _lockFileStream = new FileStream(
            Path.Combine(TempInstanceDirectory, ".lock"),
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );
        Console.WriteLine($"Created instance lock: {TempInstanceDirectory}");
    }
}
