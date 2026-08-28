using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace QuikytLoader.AvaloniaUI.Views.Common;

public partial class TextWithDetails : UserControl
{
    public static readonly StyledProperty<Control?> LeftSideActionContentProperty =
        AvaloniaProperty.Register<TextWithDetails, Control?>(
            nameof(LeftSideActionContent));

    public Control? LeftSideActionContent
    {
        get => GetValue(LeftSideActionContentProperty);
        set => SetValue(LeftSideActionContentProperty, value);
    }

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<TextWithDetails, string?>(
            nameof(Message));

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly StyledProperty<Brush> MessageForegroundProperty =
        AvaloniaProperty.Register<TextWithDetails, Brush>(
            nameof(MessageForeground));

    public Brush MessageForeground
    {
        get => GetValue(MessageForegroundProperty);
        set => SetValue(MessageForegroundProperty, value);
    }

    public static readonly StyledProperty<string?> DetailsMessageProperty =
        AvaloniaProperty.Register<TextWithDetails, string?>(
            nameof(DetailsMessage));

    public string? DetailsMessage
    {
        get => GetValue(DetailsMessageProperty);
        set => SetValue(DetailsMessageProperty, value);
    }

    public static readonly StyledProperty<Control?> RightSideContentProperty =
        AvaloniaProperty.Register<TextWithDetails, Control?>(
            nameof(RightSideContent));

    public Control? RightSideContent
    {
        get => GetValue(RightSideContentProperty);
        set => SetValue(RightSideContentProperty, value);
    }

    public static readonly StyledProperty<bool> IsCloseButtonVisibleProperty =
        AvaloniaProperty.Register<TextWithDetails, bool>(
            nameof(IsCloseButtonVisible));

    public bool IsCloseButtonVisible
    {
        get => GetValue(IsCloseButtonVisibleProperty);
        set => SetValue(IsCloseButtonVisibleProperty, value);
    }

    public TextWithDetails() => InitializeComponent();

    private void DetailsToggleButton_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DetailsToggleButton.IsChecked != true)
        {
            DetailsFASymbolIcon.Symbol = FluentAvalonia.UI.Controls.FASymbol.ChevronRight;
            DetailsMessageGrid.IsVisible = false;
        }
        else
        {
            DetailsFASymbolIcon.Symbol = FluentAvalonia.UI.Controls.FASymbol.ChevronUp;
            DetailsMessageGrid.IsVisible = true;
        }
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e) => IsVisible = false;
}
