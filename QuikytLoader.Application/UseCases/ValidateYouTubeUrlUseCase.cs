using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Validate YouTube URL strings
/// </summary>
public class ValidateYouTubeUrlUseCase
{
    /// <summary>
    /// Checks if a YouTube URL string is valid without creating the value object.
    /// </summary>
    public bool IsValid(string url) => YouTubeUrl.Create(url).IsSuccess;
}
