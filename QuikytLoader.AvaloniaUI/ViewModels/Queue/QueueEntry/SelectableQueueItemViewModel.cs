using System;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.Application.UseCases;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

public sealed partial class SelectableQueueItemViewModel(
    QueueItem model,
    Action<Guid> proceedCallback,
    Action<Guid> cancelCallback,
    FetchManualSubtitlesUseCase fetchManualSubtitlesUseCase,
    FetchAutoSubtitlesUseCase fetchAutoSubtitlesUseCase,
    CancelSubtitlesUseCase cancelSubtitlesUseCase,
    SettingsViewModel settingsViewModel)
    : QueueItemViewModel(
        model,
        proceedCallback,
        cancelCallback,
        fetchManualSubtitlesUseCase,
        fetchAutoSubtitlesUseCase,
        cancelSubtitlesUseCase,
        settingsViewModel)
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
