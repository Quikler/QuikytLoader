using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public sealed partial class QueueGroupViewModel : QueueEntryViewModel
{
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private int _selectableCount;

    [NotifyCanExecuteChangedFor(nameof(ProceedAllCommand))]
    [ObservableProperty] private bool _canProceedAll;

    public bool IsGroupContext => true;

    public QueueGroup Model { get; }

    public IReadOnlyList<QueueItemViewModel> Items { get; }

    private readonly Action<IEnumerable<Guid>> _proceedGroupCallback;

    public string Title => Model.Title;

    public void RecomputeCounts()
    {
        var selectableCount = 0;
        var selectedCount = 0;
        var canProceedAll = false;

        foreach (var item in Items)
        {
            if (!item.IsSelectable) continue;
            selectableCount++;
            if (!item.IsSelected) continue;
            selectedCount++;

            canProceedAll = true;
        }

        SelectableCount = selectableCount;
        SelectedCount = selectedCount;
        CanProceedAll = canProceedAll;
    }

    public QueueGroupViewModel(QueueGroup model, IReadOnlyList<QueueItemViewModel> items, Action<IEnumerable<Guid>> proceedGroupCallback)
    {
        Model = model;
        Items = items;
        _proceedGroupCallback = proceedGroupCallback;

        foreach (var item in Items)
        {
            item.PropertyChanged += OnItemChanged;
        }

        RecomputeCounts();
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QueueItemViewModel.IsSelected)
            || e.PropertyName == nameof(QueueItemViewModel.IsSelectable)
            || e.PropertyName == nameof(QueueItemViewModel.Status))
            RecomputeCounts();
    }

    [RelayCommand(CanExecute = nameof(CanProceedAll))]
    private void ProceedAll() =>
        _proceedGroupCallback(
            Items.Where(i => i.CanProceed).Select(i => i.QueueItemId)
        );
}
