using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Parsers;

public interface IYoutubeVideoIdParser
{
    Result<string> Parse(string value);
}
