using System;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public sealed partial class SelectableQueueItemViewModel(QueueItem model, Action<Guid> proceedCallback)
    : QueueItemViewModel(model, proceedCallback)
{
    [NotifyCanExecuteChangedFor(nameof(ProceedCommand))]
    [ObservableProperty] private bool _isSelected;

    [NotifyCanExecuteChangedFor(nameof(ProceedCommand))]
    [ObservableProperty] private bool _isSelectable = model.CanStartDownload;

    public override bool CanProceed => IsSelected && IsSelectable;

    public override void Refresh()
    {
        base.Refresh();

        IsSelectable = Model.CanStartDownload;
    }
}
