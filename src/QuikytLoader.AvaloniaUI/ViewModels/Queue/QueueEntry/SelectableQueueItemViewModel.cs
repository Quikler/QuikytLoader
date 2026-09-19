using System;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry.Subtitles;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

public sealed partial class SelectableQueueItemViewModel(
    QueueItem model,
    Action<Guid> proceedCallback,
    Action<Guid> cancelCallback,
    Action<Guid> selectInComboBoxCallback,
    QueueItemSubtitlesViewModel queueItemSubtitlesViewModel)
    : QueueItemViewModel(
        model,
        proceedCallback,
        cancelCallback,
        selectInComboBoxCallback,
        queueItemSubtitlesViewModel)
{
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [ObservableProperty] private bool _isSelected;

    [NotifyPropertyChangedFor(nameof(CanProceed))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [ObservableProperty] private bool _isSelectable = model.CanStartDownload;

    public override bool CanProceed => IsSelected && IsSelectable;
    public override bool CanCancel => IsSelected && base.CanCancel;

    public override void Refresh()
    {
        base.Refresh();

        IsSelectable = Model.CanStartDownload;
    }
}
