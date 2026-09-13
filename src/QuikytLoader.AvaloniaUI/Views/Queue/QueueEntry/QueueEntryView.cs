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

    protected virtual void OnScrollToTop(bool willStickyHeaderBeVisible)
    {
        if (QueueList is null
            || QueueScroll is null
            || QueueScroll.Content is not Control queueScrollContent)
            throw new UnreachableException();

        // Translation should happend to ScrollViewer.Content and not ScrollViewer itself
        // because Content coordinates are absolute, so they map directly to Offset
        // without any current-scroll-position interference
        var pointInContent = this.TranslatePoint(
            Bounds.TopLeft,
            queueScrollContent) ?? throw new UnreachableException();

        var topMargin = willStickyHeaderBeVisible
            // When QueueList.StickyHeader.Bounds.Height is not initialized assign 48 by default.
            // This only happens ONE time because QueueList.StickyHeader.IsVisible is false.
            // P.S. 48 is a measured height of QueueList.StickyHeader.Bounds.Height after initialization.
            // I'm also lazy and don't want to listen for layout measure or anything lol.
            ? QueueList.StickyHeader.Bounds.Height == 0d ? 48 : QueueList.StickyHeader.Bounds.Height
            : 0d;

        QueueScroll.Offset = new(QueueScroll.Offset.X, pointInContent.Y - topMargin);
    }
}
