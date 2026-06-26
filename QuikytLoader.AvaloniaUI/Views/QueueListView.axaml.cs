using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
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

    private const double StickyOffset = 15;

    private void QueueScroll_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var groups = QueueItems
            .GetVisualDescendants()
            .OfType<QueueGroupView>()
            .ToList();

        QueueGroupView? current = null;

        foreach (var group in groups)
        {
            var position = group.TranslatePoint(
                new Point(0, 0),
                QueueScroll);

            if (position is null || position.Value.Y > StickyOffset) continue;

            // Header reached sticky zone
            current = group;
        }

        if (DataContext is QueueListViewModel vm)
            vm.StickyQueueGroup = current?.DataContext as QueueGroupViewModel;
    }
}
