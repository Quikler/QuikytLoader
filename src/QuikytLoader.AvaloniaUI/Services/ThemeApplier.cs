using System;
using System.Collections.Generic;
using Avalonia.Styling;
using QuikytLoader.Application.UseCases;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.Services;

public class ThemeApplier(ManageSettingsUseCase manageSettingsUseCase) : IThemeApplier
{
    public IReadOnlyCollection<ThemePreference> AvailableThemes { get; } = Enum.GetValues<ThemePreference>();

    public void Apply(ThemePreference themePreference)
    {
        if (Avalonia.Application.Current is null) return;

        Avalonia.Application.Current.RequestedThemeVariant = themePreference switch
        {
            ThemePreference.Dark => ThemeVariant.Dark,
            ThemePreference.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    public void ApplyFromSettings() => Apply(manageSettingsUseCase.LoadSettings().ThemePreference);
}
