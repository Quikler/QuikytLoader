namespace QuikytLoader.Application.Interfaces.Temp;

public interface ITempDirectoryService
{
    string CreateSubdirectory(params string[] directoryNames);

    void DeleteSubdirectory(params string[] directoryNames);
}
