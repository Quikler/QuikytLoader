using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;

namespace QuikytLoader.AvaloniaUI.Constants;

public static class BrushResources
{
    public static IBrush AccentFillColorDefaultBrush { get; } =
        Avalonia.Application.Current!.FindResource("AccentFillColorDefaultBrush") as Brush
            ?? throw new UnreachableException();
}
