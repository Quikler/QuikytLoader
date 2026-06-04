using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI.ViewModels;

namespace QuikytLoader.AvaloniaUI.Views;

public partial class YoutubeUrlInputCardView : UserControl
{
    public YoutubeUrlInputCardView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode && Avalonia.Application.Current is App app)
            DataContext = app.Services.GetRequiredService<YoutubeUrlInputCardViewModel>();
    }

    private void YoutubeUrlTextBox_KeyDown(object _, KeyEventArgs e)
    {
        // Only execute command when Enter key is pressed
        if (e.Key == Key.Enter && DataContext is YoutubeUrlInputCardViewModel viewModel)
        {
            if (viewModel.AddToQueueCommand.CanExecute(null))
            {
                viewModel.AddToQueueCommand.Execute(null);
            }
            e.Handled = true;
        }
    }
}
