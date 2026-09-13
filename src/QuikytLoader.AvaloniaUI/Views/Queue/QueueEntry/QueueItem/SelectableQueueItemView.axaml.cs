using System.Diagnostics;
using Avalonia.VisualTree;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;
using QuikytLoader.AvaloniaUI.Views.Queue.QueueEntry.QueueGroup;

namespace QuikytLoader.AvaloniaUI.Views.Queue.QueueEntry.QueueItem;

public partial class SelectableQueueItemView : QueueEntryView
{
    public SelectableQueueItemView() => InitializeComponent();

    protected override void OnScrollToTop(bool willStickyHeaderBeVisible)
    {
        var queueGroupView = this.FindAncestorOfType<QueueGroupView>();
        if (queueGroupView is null || queueGroupView.DataContext is not QueueGroupViewModel queueGroupViewModel)
            throw new UnreachableException();

        queueGroupViewModel.IsExpanded = true;

        base.OnScrollToTop(willStickyHeaderBeVisible);
    }
}
