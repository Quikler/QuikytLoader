using QuikytLoader.Application.Interfaces.Temp;

namespace QuikytLoader.Demo.Services;

internal sealed class DemoTempDirectoryService
    : ITempDirectoryService
{
    private static readonly string _tempDownloadDirectory = Path.Combine(Path.GetTempPath(), "QuikytLoader");

    public string CreateSubdirectory(params string[] directoryNames)
        => Path.Combine(_tempDownloadDirectory, Path.Combine(directoryNames));

    public void DeleteSubdirectory(string subdirectoryPath) { }
}
