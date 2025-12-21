namespace QuikytLoader.Domain.Common;

public sealed record Error
{
    public string Code { get; }
    public string Message { get; }

    private Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Validation(string code, string message) => new(code, message);
    public static Error NotFound(string code, string message) => new(code, message);
    public static Error Failure(string code, string message) => new(code, message);
    public static Error ExternalService(string code, string message) => new(code, message);
    public static Error Configuration(string code, string message) => new(code, message);
}
