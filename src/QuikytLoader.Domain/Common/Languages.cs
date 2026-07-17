namespace QuikytLoader.Domain.Common;

public record Language(string Iso6391Name, string DisplayName);

public static class Languages
{
    private static readonly IReadOnlyDictionary<string, Language> ByIso6393 =
        new Dictionary<string, Language>(StringComparer.OrdinalIgnoreCase)
        {
            ["eng"] = new("en", "English"),
            ["rus"] = new("ru", "Russian"),
            ["deu"] = new("de", "German"),
            ["dan"] = new("da", "Danish"),
            ["fra"] = new("fr", "French"),
            ["ita"] = new("it", "Italian"),
            ["jpn"] = new("ja", "Japanese"),
            ["kor"] = new("ko", "Korean"),
            ["nld"] = new("nl", "Dutch"),
            ["nor"] = new("no", "Norwegian"),
            ["por"] = new("pt", "Portuguese"),
            ["spa"] = new("es", "Spanish"),
            ["swe"] = new("sv", "Swedish"),
            ["zho"] = new("zh", "Chinese")
        };

    public static Language FromIso6393(string iso6393)
        => ByIso6393[iso6393];

    public static IReadOnlyCollection<Language> All => [.. ByIso6393.Values];
}
