using Avalonia;
using Avalonia.Controls;

namespace QuikytLoader.AvaloniaUI.Views;

public partial class QueueItemView : UserControl
{
    public static readonly StyledProperty<bool> IsInGroupProperty =
        AvaloniaProperty.Register<QueueItemView, bool>(nameof(IsInGroup), defaultValue: false);

    public bool IsInGroup
    {
        get => GetValue(IsInGroupProperty);
        set => SetValue(IsInGroupProperty, value);
    }

    public QueueItemView()
    {
        InitializeComponent();
    }
}
