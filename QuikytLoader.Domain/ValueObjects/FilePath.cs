namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Type-safe file path value object
/// </summary>
public record FilePath
{
    public string Value { get; }

    public FilePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("File path cannot be empty", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Gets the file name from the path
    /// </summary>
    public string GetFileName() => Path.GetFileName(Value);

    /// <summary>
    /// Gets the file name without extension
    /// </summary>
    public string GetFileNameWithoutExtension() => Path.GetFileNameWithoutExtension(Value);

    /// <summary>
    /// Gets the directory name
    /// </summary>
    public string? GetDirectoryName() => Path.GetDirectoryName(Value);

    public static implicit operator string(FilePath path) => path.Value;
    public override string ToString() => Value;
}
