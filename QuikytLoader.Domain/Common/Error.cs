namespace QuikytLoader.Domain.Common;

/// <summary>
/// Represents an error that occurred during an operation.
/// Immutable record type for value-based equality.
/// </summary>
public sealed record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public Dictionary<string, object>? Metadata { get; }

    private Error(string code, string message, ErrorType type, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata;
    }

    // Factory methods for common error types

    public static Error Validation(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.Validation, metadata);

    public static Error NotFound(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.NotFound, metadata);

    public static Error Conflict(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.Conflict, metadata);

    public static Error Failure(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.Failure, metadata);

    public static Error ExternalService(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.ExternalService, metadata);

    public static Error Configuration(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.Configuration, metadata);
}

/// <summary>
/// Categorizes errors by their nature to enable differentiated handling.
/// </summary>
public enum ErrorType
{
    /// <summary>User input validation failures</summary>
    Validation,

    /// <summary>Resource not found (file, database record, etc.)</summary>
    NotFound,

    /// <summary>Duplicate/conflict scenarios</summary>
    Conflict,

    /// <summary>General operational failures</summary>
    Failure,

    /// <summary>Third-party service failures (yt-dlp, Telegram API)</summary>
    ExternalService,

    /// <summary>Missing or invalid configuration</summary>
    Configuration
}
