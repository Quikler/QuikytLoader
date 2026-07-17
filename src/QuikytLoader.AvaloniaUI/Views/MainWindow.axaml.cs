using Avalonia.Controls;
using QuikytLoader.AvaloniaUI.Demo;

namespace QuikytLoader.AvaloniaUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        Root.Children.Add(new DemoPanel());
#endif
    }
}
