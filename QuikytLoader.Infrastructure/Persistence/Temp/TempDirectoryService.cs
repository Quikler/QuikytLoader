using QuikytLoader.Application.Interfaces.Temp;

namespace QuikytLoader.Infrastructure.Persistence.Temp;

public class TempDirectoryService : ITempDirectoryService
{
    private static readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    public string CreateSubdirectory(params string[] directoryNames)
    {
        var subdirectoryPath = Path.Combine(_tempDownloadDirectory, Path.Combine(directoryNames));
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
}
