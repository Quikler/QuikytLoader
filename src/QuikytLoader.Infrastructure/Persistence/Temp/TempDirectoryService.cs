using QuikytLoader.Application;
using QuikytLoader.Application.Interfaces.Temp;

namespace QuikytLoader.Infrastructure.Persistence.Temp;

public class TempDirectoryService(IApplication application) : ITempDirectoryService
{
    public string CreateSubdirectory(params string[] directoryNames)
    {
        var subdirectoryPath = Path.Combine(application.TempInstanceDirectory, Path.Combine(directoryNames));
        Directory.CreateDirectory(subdirectoryPath);
        return subdirectoryPath;
    }

    public void DeleteSubdirectory(string subdirectoryPath)
    {
        try { Directory.Delete(subdirectoryPath, recursive: true); } catch { }

        // Cleanup of the parent directory once it's empty.
        var parentPath = Path.GetDirectoryName(subdirectoryPath);
        if (Directory.Exists(parentPath) &&
            !Directory.EnumerateFileSystemEntries(parentPath).Any())
        {
            // TOCTOU
            try { Directory.Delete(parentPath); } catch { }
        }
    }

    public void Delete()
    {
        try
        {
            Directory.Delete(application.TempInstanceDirectory, recursive: true);
            Console.WriteLine($"'{application.TempInstanceDirectory}' deleted successfully");
        }
        catch
        {
            Console.WriteLine($"Failed to delete '{application.TempInstanceDirectory}'");
        }
    }
}
