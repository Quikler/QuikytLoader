using NTextCat;
using QuikytLoader.Application.Interfaces.LanguageIdentification;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Infrastructure.LanguageIdentification;

internal class LanguageIdentifier : ILanguageIdentifier
{
    // Language keys are stored by Iso6393 code
    private static readonly IReadOnlyDictionary<string, Language> ByIso6393 =
        new Dictionary<string, Language>(StringComparer.OrdinalIgnoreCase)
        {
            ["eng"] = Language.English,
            ["rus"] = Language.Russian,
            ["deu"] = Language.German,
            ["dan"] = Language.Danish,
            ["fra"] = Language.French,
            ["ita"] = Language.Italian,
            ["jpn"] = Language.Japanese,
            ["kor"] = Language.Korean,
            ["nld"] = Language.Dutch,
            ["nor"] = Language.Norwegian,
            ["por"] = Language.Portuguese,
            ["spa"] = Language.Spanish,
            ["swe"] = Language.Swedish,
            ["zho"] = Language.Chinese,
        };

    private static readonly RankedLanguageIdentifier Identifier = GetIdentifier();
    private static RankedLanguageIdentifier GetIdentifier()
    {
        var assembly = typeof(LanguageIdentifier).Assembly;
        using var stream = assembly.GetManifestResourceStream("Core14.profile.xml")
            ?? throw new FileNotFoundException("Core14.profile.xml embedded resource was not found");
        return new RankedLanguageIdentifierFactory().Load(stream);
    }

    public Language Identify(string text)
        => ByIso6393[
            Identifier
                .Identify(text)
                .First()
                .Item1.Iso639_3];
}
