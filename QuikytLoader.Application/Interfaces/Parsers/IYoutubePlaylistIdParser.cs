using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.Parsers;

public interface IYoutubePlaylistIdParser
{
    Result<string> Parse(string value);
}
