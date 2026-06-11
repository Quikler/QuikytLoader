using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.Services;

public partial class DownloadQueueManager : ObservableObject
{
    private readonly IDownloadQueue _queue;
    private readonly IDownloadQueueProcessor _queueProcessor;

    private readonly Dictionary<Guid, QueueItemViewModel> _itemViewModels = [];

    public DownloadQueueManager(IDownloadQueue queue, IDownloadQueueProcessor queueProcessor)
    {
        _queue = queue;
        _queue.Changed += OnQueueChanged;

        _queueProcessor = queueProcessor;
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

            case QueueEvent.ItemUpdated { Item: var item }:
                UpdateItem(item);
                break;
        }
    }

    private void AddItem(QueueItem item)
    {
        var vm = CreateItemVm(item, isInGroup: false);

        RegisterItem(vm);
        AddToUi(vm);

        _queueProcessor.Enqueue(item.Id);
    }

    private void AddGroup(QueueGroup group)
    {
        var itemVms = group.ItemIds
            .Select(_queue.GetItem)
            .Select(i => CreateItemVm(i!, isInGroup: true))
            .ToArray();

        foreach (var vm in itemVms)
        {
            RegisterItem(vm);
        }

        var groupVm = new QueueGroupViewModel(group, itemVms, ProceedGroup);
        AddToUi(groupVm);

        // should not queue here as in `AddItem` because it's a group
        // and it requires user to manually click 
        // "Proceed all" in order to queue the queueItem
    }

    private void UpdateItem(QueueItem item)
    {
        if (_itemViewModels.TryGetValue(item.Id, out var vm))
            vm.UpdateFrom(item);
    }

    /// <summary>
    /// All queue entries. Can be one queue item and a group item.
    /// </summary>
    public ObservableCollection<QueueEntryViewModel> QueueEntries { get; } = [];

    private QueueItemViewModel CreateItemVm(QueueItem item, bool isInGroup) =>
        new(item, ProceedItem, isInGroup);

    private void RegisterItem(QueueItemViewModel vm)
        => _itemViewModels[vm.QueueItemId] = vm;

    private void AddToUi(QueueEntryViewModel vm) => QueueEntries.Add(vm);

    private void ProceedItem(Guid itemId) => _queueProcessor.Proceed(itemId);

    private void ProceedGroup(IEnumerable<Guid> itemIds)
    {
        foreach (var itemId in itemIds)
        {
            ProceedItem(itemId);
        }
    }
}
