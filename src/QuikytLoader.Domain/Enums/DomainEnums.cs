namespace QuikytLoader.Domain.Enums;

public static class DomainEnums
{
    public static IReadOnlyCollection<AutoSubtitlesOption> AutoSubtitlesOptions { get; } = Enum.GetValues<AutoSubtitlesOption>();
    public static IReadOnlyCollection<ThemePreference> ThemePreferences { get; } = Enum.GetValues<ThemePreference>();
}
