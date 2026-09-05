using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

namespace QuikytLoader.AvaloniaUI.Views.Queue.QueueEntry;

public class QueueEntryView : UserControl
{
    protected QueueListView? QueueList { get; private set; }
    protected ScrollViewer? QueueScroll { get; private set; }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is QueueEntryViewModel queueEntryViewModel)
            queueEntryViewModel.ScrollToTop += OnScrollToTop;

        QueueList = this.FindAncestorOfType<QueueListView>()
            ?? throw new UnreachableException();
        QueueScroll = QueueList.QueueScroll;
    }

    protected void OnScrollToTop()
    {
        if (QueueScroll is null || QueueScroll.Content is not Control queueScrollContent)
            throw new UnreachableException();

        // Translation should happend to ScrollViewer.Content and not ScrollViewer itself
        // because Content coordinates are absolute, so they map directly to Offset
        // without any current-scroll-position interference
        var pointInContent = this.TranslatePoint(
            Bounds.TopLeft,
            queueScrollContent) ?? throw new UnreachableException();

        QueueScroll.Offset = new(QueueScroll.Offset.X, pointInContent.Y);
    }
}
