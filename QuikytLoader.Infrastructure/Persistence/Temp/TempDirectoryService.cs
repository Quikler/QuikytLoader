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
    }
}
