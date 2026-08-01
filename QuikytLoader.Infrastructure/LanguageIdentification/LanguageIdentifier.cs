using NTextCat;
using QuikytLoader.Application.Interfaces.LanguageIdentification;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Infrastructure.LanguageIdentification;

internal class LanguageIdentifier : ILanguageIdentifier
{
    private static readonly RankedLanguageIdentifier Identifier = GetIdentifier();
    private static RankedLanguageIdentifier GetIdentifier()
    {
        var assembly = typeof(LanguageIdentifier).Assembly;
        using var stream = assembly.GetManifestResourceStream("Core14.profile.xml")
            ?? throw new FileNotFoundException("Core14.profile.xml embedded resource was not found");
        return new RankedLanguageIdentifierFactory().Load(stream);
    }

    public string Identify(string text)
        => Languages.FromIso6393(
            Identifier
                .Identify(text)
                .First()
                .Item1.Iso639_3)
            .Iso6391Name;
}
