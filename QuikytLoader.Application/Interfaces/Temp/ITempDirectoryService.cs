namespace QuikytLoader.Application.Interfaces.Temp;

public interface ITempDirectoryService
{
    string CreateSubdirectory(string directoryName);

    void DeleteSubdirectory(string directoryName);
}
