using System;
using System.Collections.Generic;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.ViewModels.Factories;

public class QueueEntryViewModelFactory(
    QueueItemSubtitlesViewModelFactory queueItemSubtitlesViewModelFactory)
{
    public QueueItemViewModel CreateQueueItemViewModel(
        QueueItem item,
        Action<Guid> proceedCallback,
        Action<Guid> cancelCallback)
            => new(item,
                proceedCallback,
                cancelCallback,
                queueItemSubtitlesViewModelFactory.Create(item.Subtitles));

    public SelectableQueueItemViewModel CreateSelectableQueueItemViewModel(
        QueueItem item,
        Action<Guid> proceedCallback,
        Action<Guid> cancelCallback)
            => new(item,
                proceedCallback,
                cancelCallback,
                queueItemSubtitlesViewModelFactory.Create(item.Subtitles));

    public QueueGroupViewModel CreateQueueGroupViewModel(
        QueueGroup queueGroup,
        SelectableQueueItemViewModel[] selectableQueueItemViewModels,
        Action<IEnumerable<Guid>> proceedGroupCallback)
            => new(queueGroup,
                selectableQueueItemViewModels,
                proceedGroupCallback);
}
