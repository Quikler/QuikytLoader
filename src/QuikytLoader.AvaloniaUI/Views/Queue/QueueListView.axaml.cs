using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.AvaloniaUI.ViewModels.Queue;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;
using QuikytLoader.AvaloniaUI.Views.Queue.QueueEntry.QueueGroup;
using QuikytLoader.AvaloniaUI.Views.Queue.QueueEntry.QueueItem;

namespace QuikytLoader.AvaloniaUI.Views.Queue;

public partial class QueueListView : UserControl
{
    public QueueListView()
    {
        InitializeComponent();

        if (Design.IsDesignMode || Avalonia.Application.Current is not App app)
            return;

        var dataContext = app.Services.GetRequiredService<QueueListViewModel>();
        DataContext = dataContext;
        dataContext.QueueManager.ScrollRequested += (item, scrollPosition) =>
        {
            var itemViewModel = dataContext.QueueManager.QueueEntries.First(qe => qe.ModelId == item.Id);
            var itemToFindBy = itemViewModel switch
            {
                SelectableQueueItemViewModel => dataContext.QueueManager.QueueEntries
                    .OfType<QueueGroupViewModel>()
                    .First(qgvm => qgvm.Items.Contains(itemViewModel)),
                _ => itemViewModel
            };

            if (QueueItems.ContainerFromItem(itemToFindBy) is not ContentPresenter groupOrItemPresenter)
                return;

            if (groupOrItemPresenter.Child is QueueItemView queueItemView)
                Scroll(queueItemView, scrollPosition);
            else if (groupOrItemPresenter.Child is QueueGroupView queueGroupView)
            {
                var itemsControl = queueGroupView
                    .GetVisualDescendants()
                    .OfType<ItemsControl>()
                    .First(i => i.Name == "QueueGroupItems");

                if (itemsControl.ContainerFromItem(itemViewModel) is not ContentPresenter selectableItemPresenter)
                    return;

                if (selectableItemPresenter.Child is SelectableQueueItemView selectableQueueItemView)
                    Scroll(selectableQueueItemView, scrollPosition);
            }
        };
    }

    private void Scroll(UserControl queueItemView, int scrollPosition)
    {
        var subtitlesContentTextBlock =
            queueItemView.FindDescendantOfType<QueueItemSubtitlesView>()!
                .GetVisualDescendants()
                .OfType<SelectableTextBlock>()
                .First(c => c.Name == "SubtitlesContent");

        var startRect = subtitlesContentTextBlock.TextLayout.HitTestTextPosition(scrollPosition);

        var pointInScrollViewer =
            subtitlesContentTextBlock.TranslatePoint(
                startRect.TopLeft,
                QueueScroll);

        if (pointInScrollViewer is null) return;

        var textTop = pointInScrollViewer.Value.Y;
        var textBottom = textTop + startRect.Height;

        const double margin = 30;

        // If the selected text is already completely inside the viewport, don't scroll
        if (textTop >= margin && textBottom <= QueueScroll.Viewport.Height - margin)
            return;

        // Top/bottom margin
        var offsetY = textTop < margin
            ? textTop - margin
            : textBottom - QueueScroll.Viewport.Height + margin;

        QueueScroll.Offset = new(
            QueueScroll.Offset.X,
            QueueScroll.Offset.Y + offsetY);
    }

    private const double StickyOffset = 15;

    private void QueueScroll_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var panel = QueueItems.Presenter?.Panel;
        if (panel is null) return;

        QueueGroupView? current = null;

        foreach (var container in panel.Children)
        {
            if (container.DataContext is not QueueGroupViewModel) continue;

            var groupView = container.GetVisualChildren().OfType<QueueGroupView>().FirstOrDefault();
            if (groupView is null) continue;

            var position = groupView.TranslatePoint(
                new Point(0, 0),
                QueueScroll);

            if (position is null) continue;
            if (position.Value.Y > StickyOffset) break;

            // Header reached sticky zone
            current = groupView;
        }

        if (DataContext is QueueListViewModel vm)
            vm.StickyQueueGroup = current?.DataContext as QueueGroupViewModel;
    }
}
