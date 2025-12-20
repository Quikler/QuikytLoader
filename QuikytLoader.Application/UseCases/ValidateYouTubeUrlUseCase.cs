using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Validate YouTube URL strings
/// Provides application boundary for URL validation logic
/// </summary>
public class ValidateYouTubeUrlUseCase
{
    /// <summary>
    /// Validates a YouTube URL string and returns the value object if valid.
    /// </summary>
    public Result<YouTubeUrl> Execute(string url) => YouTubeUrl.Create(url);

    /// <summary>
    /// Checks if a YouTube URL string is valid without creating the value object.
    /// </summary>
    public bool IsValid(string url) => YouTubeUrl.Create(url).IsSuccess;
}
