using QuikytLoader.Domain.Common;

namespace QuikytLoader.Application.Interfaces.LanguageIdentification;

public interface ILanguageIdentifier
{
    public Language Identify(string text);
}
