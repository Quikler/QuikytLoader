using Avalonia.Styling;
using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.Services;

public class ThemeApplier : IThemeApplier
{
    private readonly IUserSettings _userSettings;

    public ThemeApplier(IUserSettings userSettings)
    {
        _userSettings = userSettings;
        _userSettings.Changed += args =>
        {
            if (args.OldSettings.ThemePreference != args.NewSettings.ThemePreference)
                Apply(args.NewSettings.ThemePreference);
        };
    }

    public void ApplyFromSettings() => Apply(_userSettings.Current.ThemePreference);

    private void Apply(ThemePreference themePreference)
    {
        if (Avalonia.Application.Current is null) return;

        Avalonia.Application.Current.RequestedThemeVariant = themePreference switch
        {
            ThemePreference.Dark => ThemeVariant.Dark,
            ThemePreference.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }
}
