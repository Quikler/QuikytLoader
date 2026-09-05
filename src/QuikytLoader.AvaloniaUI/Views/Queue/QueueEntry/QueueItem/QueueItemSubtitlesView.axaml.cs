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
        if (_queueScroll is null)
            throw new UnreachableException();

        var startRect = SubtitlesContent.TextLayout.HitTestTextPosition(scrollPosition);

        var pointInScrollViewer =
            SubtitlesContent.TranslatePoint(
                startRect.TopLeft,
                _queueScroll) ?? throw new UnreachableException();

        var textTop = pointInScrollViewer.Y;
        var textBottom = textTop + startRect.Height;

        const double margin = 30;

        // If the selected text is already completely inside the viewport, don't scroll
        if (textTop >= margin && textBottom <= _queueScroll.Viewport.Height - margin)
            return;

        // Top/bottom margin
        var offsetY = textTop < margin
            ? textTop - margin
            : textBottom - _queueScroll.Viewport.Height + margin;

        _queueScroll.Offset = new(
            _queueScroll.Offset.X,
            _queueScroll.Offset.Y + offsetY);
    }
}
