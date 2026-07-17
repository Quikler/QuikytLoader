using QuikytLoader.Application.Interfaces.Parsers;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.AvaloniaUI.Validators;

public sealed class YoutubeUrlValidator(
    IYoutubeVideoIdParser videoParser,
    IYoutubePlaylistIdParser playlistParser)
    : IYoutubeValidator
{
    public Result Validate(string value)
    {
        var playlistResult = playlistParser.Parse(value);
        if (playlistResult.IsSuccess)
            return Result.Success();

        var videoResult = videoParser.Parse(value);
        if (videoResult.IsSuccess)
            return Result.Success();

        return Result.Failure("Invalid Youtube URL.");
    }
}

internal interface IYoutubeValidator
{
    Result Validate(string value);
}
