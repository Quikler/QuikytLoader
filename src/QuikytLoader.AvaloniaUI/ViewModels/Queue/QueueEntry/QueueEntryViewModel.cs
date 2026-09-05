using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

public abstract class QueueEntryViewModel(Guid modelId) : ObservableObject
{
    public Guid ModelId { get; } = modelId;

    public event Action? ScrollToTop;
    public void RaiseScrollToTop()
        => ScrollToTop?.Invoke();
}
