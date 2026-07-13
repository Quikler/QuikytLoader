using NTextCat;

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
        => Iso639Converter.ConvertToIso6391(
            Identifier
                .Identify(text)
                .First()
                .Item1.Iso639_3);

    private static class Iso639Converter
    {
        private static readonly Dictionary<string, string> Iso6393ToIso6391 =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "dan", "da" }, // Danish
                { "deu", "de" }, // German
                { "eng", "en" }, // English
                { "fra", "fr" }, // French
                { "ita", "it" }, // Italian
                { "jpn", "ja" }, // Japanese
                { "kor", "ko" }, // Korean
                { "nld", "nl" }, // Dutch
                { "nor", "no" }, // Norwegian
                { "por", "pt" }, // Portuguese
                { "rus", "ru" }, // Russian
                { "spa", "es" }, // Spanish
                { "swe", "sv" }, // Swedish
                { "zho", "zh" }  // Chinese
            };

        public static string ConvertToIso6391(string iso6393)
            => Iso6393ToIso6391[iso6393];
    }
}
