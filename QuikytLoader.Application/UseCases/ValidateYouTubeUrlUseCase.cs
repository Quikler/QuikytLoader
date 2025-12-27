using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Validate YouTube URL strings
/// </summary>
public class ValidateYouTubeUrlUseCase
{
    public bool IsValid(string url) => YouTubeUrl.Create(url).IsSuccess;
}
