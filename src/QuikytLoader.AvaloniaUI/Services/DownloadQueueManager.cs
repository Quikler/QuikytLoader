using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.AvaloniaUI.ViewModels.Factories;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.Services;

public partial class DownloadQueueManager : ObservableObject
{
    private readonly IDownloadQueue _queue;
    private readonly IDownloadQueueProcessor _queueProcessor;
    private readonly QueueEntryViewModelFactory _queueEntryViewModelFactory;

    private readonly Dictionary<Guid, QueueItemViewModel> _itemViewModels = [];

    private int? _firstGroupIndex;

    /// <summary>
    /// All queue entries. Can be one queue item and a group item.
    /// </summary>
    public ObservableCollection<QueueEntryViewModel> QueueEntries { get; } = [];

    /// <summary>
    /// Flattens QueueItemViewModels from items and groups in QueueEntries
    /// </summary>
    public IReadOnlyList<QueueItemViewModel> QueueItems =>
        [.. QueueEntries
            .SelectMany<QueueEntryViewModel, QueueItemViewModel>(e => e switch
            {
                QueueItemViewModel item => [item],
                QueueGroupViewModel group => group.Items,
                _ => []
            })];

    private QueueItemViewModel? _selectedQueueItem;
    public QueueItemViewModel? SelectedQueueItem
    {
        get => _selectedQueueItem;
        set
        {
            // Checking for null because when QueueItems change
            // the ComboBox in QueueListView sets it's SelectedItem to null
            // due to it's ItemsSource change which is not what we want
            if (_selectedQueueItem == value || value is null) return;

            // Remove border for previously selected item
            _selectedQueueItem?.RaiseRemoveBorder();
            _selectedQueueItem = value;
            OnPropertyChanged();

            // Add border for currently selected item
            _selectedQueueItem.RaiseAddBorder();

            // If selected item is in group (meaning it's type is SelectableQueueItemViewModel)
            // or there is a group before this item in QueueEntries
            // then the group header is/will be visible
            var willStickyHeaderBeVisible = _selectedQueueItem is SelectableQueueItemViewModel
                || QueueEntries.IndexOf(_selectedQueueItem) > _firstGroupIndex;
            _selectedQueueItem.RaiseScrollToTop(willStickyHeaderBeVisible);
        }
    }

    public DownloadQueueManager(
        IDownloadQueue queue,
        IDownloadQueueProcessor queueProcessor,
        QueueEntryViewModelFactory queueEntryViewModelFactory)
    {
        _queue = queue;
        _queue.Changed += OnQueueChanged;

        _queueProcessor = queueProcessor;
        _queueEntryViewModelFactory = queueEntryViewModelFactory;

        QueueEntries.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(QueueItems));
            SelectedQueueItem ??= QueueItems.First();
        };
    }

    private void OnQueueChanged(QueueEvent evt)
    {
        switch (evt)
        {
            case QueueEvent.ItemAdded { Item: var item }:
                AddItem(item);
                break;

            case QueueEvent.GroupAdded { Group: var group }:
                AddGroup(group);
                break;

            case QueueEvent.ItemUpdated { ItemId: var itemId }:
                UpdateItem(itemId);
                break;
        }
    }

    private void AddItem(QueueItem item)
    {
        var itemVm = _queueEntryViewModelFactory.CreateQueueItemViewModel(
            item,
            ProceedItem,
            CancelItem,
            SelectInComboBox);

        RegisterItem(itemVm);
        AddToUi(itemVm);

        _queueProcessor.Enqueue(item.Id);
    }

    private void AddGroup(QueueGroup group)
    {
        var itemVms = group.ItemIds
            .Select(_queue.GetItem)
            .Select(item => _queueEntryViewModelFactory.CreateSelectableQueueItemViewModel(
                item,
                ProceedItem,
                CancelItem,
                SelectInComboBox))
            .ToArray();

        foreach (var vm in itemVms)
        {
            RegisterItem(vm);
        }

        var groupVm = _queueEntryViewModelFactory.CreateQueueGroupViewModel(group, itemVms, ProceedGroup);
        AddToUi(groupVm);
        _firstGroupIndex ??= QueueEntries.Count - 1;

        // should not queue here as in `AddItem` because it's a group
        // and it requires user to manually click 
        // "Proceed all" in order to queue the queueItem
    }

    private void UpdateItem(Guid itemId)
    {
        if (_itemViewModels.TryGetValue(itemId, out var vm))
            vm.Refresh();
    }

    private void RegisterItem(QueueItemViewModel vm)
        => _itemViewModels[vm.ModelId] = vm;

    private void AddToUi(QueueEntryViewModel vm) => QueueEntries.Add(vm);

    private void ProceedItem(Guid itemId)
        => _queueProcessor.Proceed(itemId);

    private void CancelItem(Guid itemId)
        => _queueProcessor.Cancel(itemId);

    private void ProceedGroup(IEnumerable<Guid> itemIds)
    {
        foreach (var itemId in itemIds)
        {
            ProceedItem(itemId);
        }
    }

    private void SelectInComboBox(Guid itemId)
        => SelectedQueueItem = _itemViewModels[itemId];
}
