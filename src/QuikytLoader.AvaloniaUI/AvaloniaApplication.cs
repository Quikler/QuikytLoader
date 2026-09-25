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
        CleanupOrphanedInstances();
        CreateInstanceLock();

        AppBuilder.Configure(() => new App(_services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }

    private void CleanupOrphanedInstances()
    {
        if (!Directory.Exists(_tempDirectory)) return;

        var instanceDirectories = Directory.GetDirectories(_tempDirectory);
        foreach (string instanceDirectory in instanceDirectories)
        {
            string lockFile = Path.Combine(instanceDirectory, ".lock");
            Console.WriteLine(lockFile);
            try
            {
                // Try to open lock file exclusively - if it works, it's orphaned
                File.Open(lockFile, FileMode.Open, FileAccess.Write, FileShare.None).Dispose();
                Console.WriteLine($"Cleaned up orphaned instance: {Path.GetFileName(instanceDirectory)}");
                Directory.Delete(instanceDirectory, recursive: true);
            }
            catch (IOException)
            {
                // Lock file is in use by another instance, skip
                Console.WriteLine($"Skipped active instance: {Path.GetFileName(instanceDirectory)}");
            }
        }
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
