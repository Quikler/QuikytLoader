namespace QuikytLoader.Domain.Common;

public readonly record struct Language(string Iso6391Code, string DisplayName)
{
    public static readonly Language English = new("en", "English");
    public static readonly Language Russian = new("ru", "Russian");
    public static readonly Language German = new("de", "German");
    public static readonly Language Danish = new("da", "Danish");
    public static readonly Language French = new("fr", "French");
    public static readonly Language Italian = new("it", "Italian");
    public static readonly Language Japanese = new("ja", "Japanese");
    public static readonly Language Korean = new("ko", "Korean");
    public static readonly Language Dutch = new("nl", "Dutch");
    public static readonly Language Norwegian = new("no", "Norwegian");
    public static readonly Language Portuguese = new("pt", "Portuguese");
    public static readonly Language Spanish = new("es", "Spanish");
    public static readonly Language Swedish = new("sv", "Swedish");
    public static readonly Language Chinese = new("zh", "Chinese");

    public static readonly IReadOnlyCollection<Language> Options =
    [
        English,
        Russian,
        German,
        Danish,
        French,
        Italian,
        Japanese,
        Korean,
        Dutch,
        Norwegian,
        Portuguese,
        Spanish,
        Swedish,
        Chinese
    ];
}
