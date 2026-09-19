using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

public abstract class QueueEntryViewModel(Guid modelId) : ObservableObject
{
    private readonly List<Action> _pendingActions = [];
    private bool _isAssociatedViewLoaded;

    public Guid ModelId { get; } = modelId;

    public void RaisePendingActions()
    {
        _isAssociatedViewLoaded = true;
        foreach (var action in _pendingActions)
        {
            action();
        }
    }

    public event Action<bool>? ScrollToTop;
    public void RaiseScrollToTop(bool willStickyHeaderBeVisible)
        => RaiseEventIfLoaded(() => RaiseScrollToTop(willStickyHeaderBeVisible), () => ScrollToTop?.Invoke(willStickyHeaderBeVisible));

    public event Action? AddBorder;
    public void RaiseAddBorder()
        => RaiseEventIfLoaded(RaiseAddBorder, AddBorder);

    public event Action? RemoveBorder;
    public void RaiseRemoveBorder()
        => RaiseEventIfLoaded(RaiseRemoveBorder, RemoveBorder);

    private void RaiseEventIfLoaded(Action raiseAction, Action? @event)
    {
        if (!_isAssociatedViewLoaded)
        {
            _pendingActions.Add(raiseAction);
            return;
        }

        @event?.Invoke();
    }
}
