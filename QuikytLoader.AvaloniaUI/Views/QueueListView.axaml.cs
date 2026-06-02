using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI.ViewModels;

namespace QuikytLoader.AvaloniaUI.Views;

public partial class QueueListView : UserControl
{
    public QueueListView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode && Avalonia.Application.Current is App app)
            DataContext = app.Services.GetRequiredService<QueueListViewModel>();
    }
}
