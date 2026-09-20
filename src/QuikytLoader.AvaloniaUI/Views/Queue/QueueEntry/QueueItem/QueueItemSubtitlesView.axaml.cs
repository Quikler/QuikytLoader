using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry.Subtitles;

namespace QuikytLoader.AvaloniaUI.Views.Queue.QueueEntry.QueueItem;

public sealed partial class QueueItemSubtitlesView : UserControl
{
    private QueueListView? _queueList;
    private ScrollViewer? _queueScroll;

    public QueueItemSubtitlesView() => InitializeComponent();

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is QueueItemSubtitlesViewModel queueItemSubtitlesViewModel)
            queueItemSubtitlesViewModel.ScrollInSubtitles += OnScrollInSubtitles;

        _queueList = this.FindAncestorOfType<QueueListView>()
            ?? throw new UnreachableException();
        _queueScroll = _queueList.QueueScroll;
    }

    private void OnScrollInSubtitles(int scrollPosition)
    {
        if (_queueList is null
            || _queueScroll is null
            || _queueScroll.Content is not Control queueScrollContent)
            throw new UnreachableException();

        var startRect = SubtitlesContent.TextLayout.HitTestTextPosition(scrollPosition);

        var pointInContent =
            SubtitlesContent.TranslatePoint(
                startRect.TopLeft,
                queueScrollContent) ?? throw new UnreachableException();

        if (!IsFullyVisible(pointInContent, startRect.Height))
        {
            double topMargin = _queueList.StickyHeader.IsVisible
                ? _queueList.StickyHeader.Bounds.Height
                : 0d;

            _queueScroll.Offset = new(_queueScroll.Offset.X, pointInContent.Y - topMargin);
        }
    }

    private bool IsFullyVisible(Point pointInContent, double selectionHeight)
    {
        var targetY = pointInContent.Y;
        var targetTop = targetY;
        var targetBottom = targetY + selectionHeight;

        // Check if target position is already visible in the viewport
        var viewportTop = _queueScroll!.Offset.Y;
        var viewportBottom = _queueScroll.Offset.Y + _queueScroll.Bounds.Height;
        return targetTop >= viewportTop && targetBottom <= viewportBottom;
    }
}
