using System.Collections.Generic;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.Services;

public interface IThemeApplier
{
    IReadOnlyCollection<ThemePreference> AvailableThemes { get; }

    void Apply(ThemePreference themePreference);

    void ApplyFromSettings();
}
