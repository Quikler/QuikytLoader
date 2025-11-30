using Avalonia.Controls;
using Avalonia.Input;
using QuikytLoader.ViewModels;

namespace QuikytLoader.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        // Only execute command when Enter key is pressed
        if (e.Key == Key.Enter && DataContext is HomeViewModel viewModel)
        {
            if (viewModel.AddToQueueCommand.CanExecute(null))
            {
                viewModel.AddToQueueCommand.Execute(null);
            }
            e.Handled = true;
        }
    }
}
