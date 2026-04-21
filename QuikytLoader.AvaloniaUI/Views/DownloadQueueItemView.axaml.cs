using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace QuikytLoader.AvaloniaUI.Views;

public partial class DownloadQueueItemView : UserControl
{
    public static readonly StyledProperty<ICommand?> ProceedCommandProperty =
        AvaloniaProperty.Register<DownloadQueueItemView, ICommand?>(nameof(ProceedCommand));

    public ICommand? ProceedCommand
    {
        get => GetValue(ProceedCommandProperty);
        set => SetValue(ProceedCommandProperty, value);
    }

    public DownloadQueueItemView()
    {
        InitializeComponent();
    }
}
