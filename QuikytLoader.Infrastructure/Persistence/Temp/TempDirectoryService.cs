using QuikytLoader.Application.Interfaces.Temp;

namespace QuikytLoader.Infrastructure.Persistence.Temp;

public class TempDirectoryService : ITempDirectoryService
{
    private static readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    public string CreateSubdirectory(string directoryName)
    {
        var subdirectoryPath = Path.Combine(_tempDownloadDirectory, directoryName);
        Directory.CreateDirectory(subdirectoryPath);
        return subdirectoryPath;
    }

    public void DeleteSubdirectory(string directoryName)
    {
        var subdirectoryPath = Path.Combine(_tempDownloadDirectory, directoryName);
        try { Directory.Delete(subdirectoryPath, recursive: true); } catch { }

        // Clean up now-empty parent directories left behind (e.g. the video id
        // directory that only exists to hold "media"/"subtitles" subdirectories)
        var parentDirectory = Directory.GetParent(subdirectoryPath);
        while (parentDirectory is not null &&
               parentDirectory.Exists &&
               !string.Equals(parentDirectory.FullName.TrimEnd(Path.DirectorySeparatorChar), _tempDownloadDirectory.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) &&
               !parentDirectory.EnumerateFileSystemInfos().Any())
        {
            try { parentDirectory.Delete(); } catch { break; }
            parentDirectory = parentDirectory.Parent;
        }
    }
}
