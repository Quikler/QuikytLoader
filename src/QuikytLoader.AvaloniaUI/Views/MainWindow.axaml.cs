using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using QuikytLoader.AvaloniaUI.Demo;
using QuikytLoader.AvaloniaUI.Views.Common;
using QuikytLoader.AvaloniaUI.Views.Queue;

namespace QuikytLoader.AvaloniaUI.Views;

public partial class MainWindow : Window
{
    private SearchTextBox? _searchTextBox;

    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        Root.Children.Add(new DemoPanel());
#endif
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            // Esc
            case Key.Escape:
                FocusManager.Focus(null);
                break;

            // Ctrl + F
            case Key.F when e.KeyModifiers == KeyModifiers.Control:
                _searchTextBox ??= this.FindDescendantOfType<QueueListView>()?.SearchTextBox
                    ?? throw new UnreachableException();

                _searchTextBox.Focus();
                break;

            default: return;
        }

        e.Handled = true;
    }
}
