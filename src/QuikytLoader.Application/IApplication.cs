namespace QuikytLoader.Application;

public interface IApplication
{
    string TempInstanceDirectory { get; }

    Guid InstanceId { get; }

    void Run(string[] args);
}
